using InternProject.Dtos;
using InternProject.Models.PagingModels;

namespace InternProject.Services.BookMarkService
{
    public interface IBookMarkService
    {
        Task AddBookMarkAsync(Guid postId, CancellationToken cancellationToken);
        Task RemoveBookMarkAsync(Guid postId, CancellationToken cancellationToken);
        Task <PaginationModel<BookMarkPageDto>> GetBookMarksAsync(
            int pageNumber, 
            int pageSize,
            CancellationToken cancellationToken);
    }
}
