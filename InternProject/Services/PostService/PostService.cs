using Azure;
using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Models.ImageModels;
using InternProject.Models.PagingModels;
using InternProject.Services.ImageService;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.PostService
{
    public class PostService(AppDbContext context,IUserContext userContext,ImageQueue imageQueue,IImageCleanupService imageCleanupService) : IPostService
    {
        public async Task<PostResponseDto> CreatePostAsync(PostCreateDto dto, CancellationToken ct)
        {
            if (dto.PostImages == null || dto.PostImages.Count == 0)
                throw new ApiException(
                    "POST_IMAGES_REQUIRED",
                    null,
                    StatusCodes.Status400BadRequest
                );

            if (dto.PostImages.Count > ImageUploadRules.MaxImagesPerPost)
                throw new ApiException(
                    "IMAGE_LIMIT_EXCEEDED",
                    new { max = ImageUploadRules.MaxImagesPerPost },
                    StatusCodes.Status400BadRequest
                );

            foreach (var base64 in dto.PostImages)
            {
                ValidateAndDecodeBase64(base64);
            }

            using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var post = PostMappings.ToModel(dto);
                post.SellerId = userContext.GetCurrentUserId();

                await context.Posts.AddAsync(post, ct);
                await context.SaveChangesAsync(ct); // PostId generated

                var images = dto.PostImages.Select(_ =>
                    ImageMappings.ToModel(
                        string.Empty,
                        post.PostId,
                        ImageOwnerType.Item
                    )
                ).ToList();
                await context.Images.AddRangeAsync(images, ct);
                await context.SaveChangesAsync(ct); // ImageIds generated

                // Enqueue background jobs (order preserved)
                for (int i = 0; i < images.Count; i++)
                {
                    imageQueue.Enqueue(images[i].ImageId, dto.PostImages[i]);
                }

                await transaction.CommitAsync(ct);
                return PostMappings.ToDto(post, images);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        private static void ValidateAndDecodeBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ApiException("INVALID_IMAGE", null, StatusCodes.Status400BadRequest);

            try
            {
                var bytes = Convert.FromBase64String(base64);

                if (bytes.Length > ImageUploadRules.MaxImageSizeBytes)
                    throw new ApiException(
                        "IMAGE_TOO_LARGE",
                        new { maxSizeMb = 5 },
                        StatusCodes.Status400BadRequest
                    );
            }
            catch (FormatException)
            {
                throw new ApiException(
                    "INVALID_IMAGE_FORMAT",
                    null,
                    StatusCodes.Status400BadRequest
                );
            }
        }

        public async Task DeletePostAsync(Guid postId, CancellationToken cancellationToken)
        {
            var post = await context.Posts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PostId == postId, cancellationToken) ??
                throw new ApiException("POST_NOT_FOUND", null, StatusCodes.Status404NotFound);

            if (post.SellerId != userContext.GetCurrentUserId())
                throw new ApiException("FORBIDDEN", null, StatusCodes.Status403Forbidden);

           
            await imageCleanupService.DeleteImagesAsync(post.Images, cancellationToken);

            
            context.Posts.Remove(post);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PostResponseDto> UpdatePostAsync(
            Guid postId,
            PostUpdateDto dto,
            CancellationToken ct)
        {
            var post = await context.Posts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PostId == postId, ct)
                ?? throw new ApiException("POST_NOT_FOUND", null, StatusCodes.Status404NotFound);

            if (post.SellerId != userContext.GetCurrentUserId())
                throw new ApiException("FORBIDDEN", null, StatusCodes.Status403Forbidden);

            var keepImages = dto.KeepImages ?? [];
            var newImagesInput = dto.NewImages ?? [];

            var totalImages = keepImages.Count + newImagesInput.Count;
            if (totalImages > ImageUploadRules.MaxImagesPerPost)
                throw new ApiException(
                    "IMAGE_LIMIT_EXCEEDED",
                    new { max = ImageUploadRules.MaxImagesPerPost },
                    StatusCodes.Status400BadRequest
                );

            foreach (var base64 in newImagesInput)
            {
                ValidateAndDecodeBase64(base64);
            }

            List<Images> imagesToDelete;

            using var tx = await context.Database.BeginTransactionAsync(ct);

            try
            {
                PostMappings.UpdateModel(post, dto);

                var existingImageIds = post.Images.Select(i => i.ImageId).ToHashSet();

                if (keepImages.Any(id => !existingImageIds.Contains(id)))
                    throw new ApiException("INVALID_IMAGE_ID", null, StatusCodes.Status400BadRequest);

                imagesToDelete = post.Images
                    .Where(i => !keepImages.Contains(i.ImageId))
                    .ToList();

                context.Images.RemoveRange(imagesToDelete);

                var newImages = newImagesInput.Select(_ =>
                    ImageMappings.ToModel(
                        string.Empty,
                        post.PostId,
                        ImageOwnerType.Item
                    )
                ).ToList();

                await context.Images.AddRangeAsync(newImages, ct);
                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                await imageCleanupService.DeleteImagesAsync(imagesToDelete, ct);

                for (int i = 0; i < newImages.Count; i++)
                {
                    imageQueue.Enqueue(newImages[i].ImageId, newImagesInput[i]);
                }

                var responseImages = post.Images
                    .Where(i => !imagesToDelete.Contains(i))
                    .Concat(newImages)
                    .ToList();

                return PostMappings.ToDto(post, responseImages);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<GetPostResponseDto> GetPostAsync(Guid postId, CancellationToken cancellationToken)
        {
            var post = await context.Posts
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PostId == postId, cancellationToken);

            return post == null
                ? throw new ApiException(
                    "POST_NOT_FOUND",
                    null,
                    StatusCodes.Status404NotFound
                )
                : PostMappings.ToGetDto(post);
        }

        public async Task<PaginationModel<GetPostResponseDto>> GetPostsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();

           
            if (userId == Guid.Empty)
                throw new ApiException("UNAUTHORIZED", null, StatusCodes.Status401Unauthorized);

            pageSize = pageSize > 50 ? 50 : (pageSize < 1 ? 10 : pageSize);
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var query = context.Posts
                .AsNoTracking()
                .Where(p => p.SellerId == userId)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt);

            return await query
                .Select(p => PostMappings.ToGetDto(p))
                .ToPaginatedListAsync(pageNumber, pageSize);
        }
    }
}
