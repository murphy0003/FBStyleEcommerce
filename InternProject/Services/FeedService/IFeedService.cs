using InternProject.Dtos;
using InternProject.Models.PagingModels;

namespace InternProject.Services.FeedService
{
    public interface IFeedService
    {
        Task<PaginationKeySetModel<FeedItemDto, CompositeCursor<DateTime, Guid>>> GetGlobalFeed(
                string? itemName,
                CompositeCursor<DateTime, Guid>? cursor,
                CancellationToken ct,
                int pageSize = 20);
    }
}
