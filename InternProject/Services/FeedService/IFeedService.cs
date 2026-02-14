using InternProject.Dtos;
using InternProject.Models.PagingModels;

namespace InternProject.Services.FeedService
{
    public interface IFeedService
    {
        Task<PaginationKeySetModel<FeedItemDto>> GetGlobalFeed(string? itemName,DateTime? cursor, CancellationToken ct, int pageSize=20);
    }
}
