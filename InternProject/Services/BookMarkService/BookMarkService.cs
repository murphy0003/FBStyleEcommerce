using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Models.BookMarkModels;
using InternProject.Models.ImageModels;
using InternProject.Models.PagingModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.BookMarkService
{
    public class BookMarkService(AppDbContext context, IUserContext userContext) : IBookMarkService
    {
        public async Task AddBookMarkAsync(Guid postId, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var existingBookmark = await context.BookMarks.FindAsync([userId, postId], cancellationToken);
            if (existingBookmark != null)
            {
                throw new ApiException("BOOKMARK_ALREADY_EXISTS", null, StatusCodes.Status400BadRequest);
            }
            var bookmark = new BookMark
            {
                UserId = userId,
                PostId = postId,
                SavedAt = DateTime.UtcNow
            };
            await context.BookMarks.AddAsync(bookmark, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
         public async Task RemoveBookMarkAsync(Guid postId, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var bookmark = await context.BookMarks.FindAsync([userId, postId], cancellationToken)
                ?? throw new ApiException("BOOKMARK_NOT_FOUND", null, StatusCodes.Status404NotFound);
            context.BookMarks.Remove(bookmark);
            await context.SaveChangesAsync(cancellationToken);
        }
        public async Task<PaginationModel<BookMarkPageDto>> GetBookMarksAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken)
        {
            var currentUserId = userContext.GetCurrentUserId();

            if (pageNumber <= 0)
                throw new ApiException(
                    "Page number must be greater than 0.",
                    null,
                    StatusCodes.Status400BadRequest);

            if (pageSize <= 0 || pageSize > 50)
                throw new ApiException(
                    "Page size must be between 1 and 50.",
                    null,
                    StatusCodes.Status400BadRequest);

            var query = context.BookMarks
                .AsNoTracking()
                .Where(b => b.UserId == currentUserId && b.Post != null)
                .OrderByDescending(b => b.SavedAt)
                .Select(b => new BookMarkPageDto(
                    b.PostId,
                    b.Post.ItemName,
                    b.Post.Price,
                    b.Post.Images
                        .Where(img => img.ImageOwnerType == ImageOwnerType.Item)
                        .OrderBy(img => img.CreatedAt)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()!,
                    b.Post.CreatedAt ?? DateTime.MinValue,
                    true
                ));

            return await query.ToPaginatedListAsync(pageNumber, pageSize);
        }
    }
}
