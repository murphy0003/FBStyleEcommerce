using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.PagingModels;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.FeedService
{
    public class FeedService(AppDbContext appDbContext) : IFeedService
    {
        public async Task<PaginationKeySetModel<FeedItemDto,
                CompositeCursor<DateTime, Guid>>> GetGlobalFeed(
                string? itemName,
                CompositeCursor<DateTime, Guid>? cursor,
                CancellationToken ct,
                int pageSize = 20)
        {
                pageSize = Math.Clamp(pageSize, 1, 100);
                var query = appDbContext.Posts
                .AsNoTracking()
                .Where(p =>
                    string.IsNullOrWhiteSpace(itemName) ||
                    EF.Functions.Like(p.ItemName, $"%{itemName}%"))
                    .ProjectToFeedDto();

                return await query.ToKeySetPaginatedListAsync(
                pageSize,
                cursor,
                x => x.Postdata.CreatedAt,
                x => x.Postdata.PostId
                );
        }
    }
}
