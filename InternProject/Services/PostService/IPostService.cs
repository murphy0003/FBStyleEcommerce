using InternProject.Dtos;
using InternProject.Models.PagingModels;

namespace InternProject.Services.PostService
{
    public interface IPostService
    {
        Task<PostResponseDto> CreatePostAsync(PostCreateDto postCreateDto , CancellationToken cancellationToken);
        Task DeletePostAsync (Guid postId, CancellationToken cancellationToken); 
        Task<PostResponseDto> UpdatePostAsync(Guid postId , PostUpdateDto postUpdateDto , CancellationToken cancellationToken);
        Task<GetPostResponseDto> GetPostAsync (Guid postId, CancellationToken cancellationToken);
        Task<PaginationModel<GetPostResponseDto>> GetPostsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    }
}
