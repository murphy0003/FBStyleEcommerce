using InternProject.Dtos;
using InternProject.Models.ImageModels;
using InternProject.Models.PostModels;
using Microsoft.Extensions.Hosting;

namespace InternProject.Extensions
{
    public static class PostMappings
    {
        public static Posts ToModel(PostCreateDto postCreateDto)
        {
            return new Posts
            {
                ItemName = postCreateDto.ItemName,
                Price = postCreateDto.Price,
                Description = postCreateDto.Description,
                ItemCondition = Enum.Parse<ItemCondition>(postCreateDto.ItemCondition),
                ItemStatus = Enum.Parse<ItemStatus>(postCreateDto.ItemStatus),
                CreatedAt = DateTime.UtcNow
            };
        }
        public static PostResponseDto ToDto(Posts post, List<Images> images)
        {
            return new PostResponseDto(
                post.PostId,
                post.ItemName,
                post.Description!,
                post.Price,
                post.ItemStatus.ToString(),
                post.ItemCondition.ToString(),
                [.. images.Select(i => new ImageResultDto(i.ImageId, i.Status))]
            );
        }
        public static Posts UpdateModel(Posts posts, PostUpdateDto postUpdateDto)
        {
            if (postUpdateDto.ItemName is not null)
                posts.ItemName = postUpdateDto.ItemName.Trim();

            if (postUpdateDto.Description is not null)
                posts.Description = postUpdateDto.Description.Trim();

            if (!string.IsNullOrWhiteSpace(postUpdateDto.ItemCondition) && Enum.TryParse<ItemCondition>(
                postUpdateDto.ItemCondition,
                ignoreCase: true,
                out var itemCondition
            ))
            {     posts.ItemCondition = itemCondition; }
           
            if (!string.IsNullOrWhiteSpace(postUpdateDto.ItemStatus) && Enum.TryParse<ItemStatus>(
                postUpdateDto.ItemStatus,
                ignoreCase:true,
                out var itemStatus))
            {  posts.ItemStatus = itemStatus; }

            posts.UpdatedAt = DateTime.UtcNow;
            return posts;
        }
        public static GetPostResponseDto ToGetDto(Posts post)
        {
            return new GetPostResponseDto(
                post.PostId,
                post.ItemName,
                post.Description!,
                post.Price,
                post.ItemStatus.ToString(),
                post.ItemCondition.ToString(),
                post.SellerId,
                (DateTime)post.CreatedAt!,
                (DateTime)post.UpdatedAt!,
                [.. post.Images
                    .Where(i => i.Status == ImageStatus.Completed)
                    .OrderBy(i => i.CreatedAt)
                    .Select(i => new PostImageDto(i.ImageId, i.ImageUrl , i.Status.ToString()))]
            );
        }
    }
}

