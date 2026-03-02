using System;
using System.Linq;
using InternProject.Dtos;
using InternProject.Models.ImageModels;
using InternProject.Models.PostModels;

namespace InternProject.Extensions
{
    public static class FeedMappings
    {
        public static IQueryable<FeedItemDto> ProjectToFeedDto(this IQueryable<Post> query)
        {
            return query.Select(p => new FeedItemDto(
                new PostDataDto(
                    p.PostId,
                    p.ItemName,
                    p.Price,
                    p.ItemCondition.ToString(),
                    p.ItemStatus.ToString(),
                    p.CreatedAt ?? DateTime.MinValue,
                    p.Images
                    .Where(img => img.PostId == p.PostId && img.Status == ImageStatus.Completed)
                    .Select(s => s.ImageUrl)
                ),
                new ProfileDataDto(
                    p.Seller.Profile.ProfileId,
                    p.Seller.Profile.DisplayName,
                    p.Seller.Profile.PhoneNumber,
                    p.Seller.Profile.BlueMark,
                    p.Seller.Profile.Images
                        .Where(img => img.ProfileId == p.Seller.Profile.ProfileId && img.Status == ImageStatus.Completed)
                        .OrderByDescending(img => img.CreatedAt)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                )
            ));
        }
    }
}
