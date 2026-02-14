using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.PagingModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.FeedService
{
    public class FeedService(AppDbContext appDbContext) : IFeedService
    {
        public Task<PaginationKeySetModel<FeedItemDto>> GetGlobalFeed(string? itemName , DateTime? cursor , CancellationToken ct , int pageSize=20 )
        {
            var referenceTime = cursor ?? DateTime.UtcNow;
            return appDbContext.Posts
                .AsNoTracking()
                .Where(p => p.CreatedAt < referenceTime)
                .Where(p => string.IsNullOrEmpty(itemName) || EF.Functions.Like(p.ItemName , $"%{itemName}%"))
                .OrderByDescending(p => p.CreatedAt)
                .ProjectToFeedDto()
                .ToKeySetPaginatedListAsync(pageSize, x => x.Postdata.CreatedAt);
        }
    }
}
