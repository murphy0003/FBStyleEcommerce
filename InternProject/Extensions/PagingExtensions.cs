using InternProject.Models.PagingModels;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Extensions
{
    public static class PagingExtensions
    {
        public static async Task<PaginationModel<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var count = await source.CountAsync();

            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationModel<T>(items, count, pageIndex, pageSize);
        }
    }
}
