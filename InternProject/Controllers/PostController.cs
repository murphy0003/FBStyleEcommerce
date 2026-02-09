using InternProject.Dtos;
using InternProject.Services.PostService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController(PostService postService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> CreatePost(PostCreateDto postCreateDto, CancellationToken cancellationToken)
        {
            var result = await postService.CreatePostAsync(postCreateDto, cancellationToken);
            // Implementation for creating a post goes here.
            HttpContext.Items["ResponseMessage"] = "Post created successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Accepted(result);
        }
        [HttpDelete("{postId:guid}")]
        public async Task<ActionResult> DeletePost(Guid postId, CancellationToken cancellationToken)
        {
            await postService.DeletePostAsync(postId, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Post deleted successfully";

            return NoContent();
        }
        [HttpPatch("{postId:guid}")]
        public async Task<ActionResult> UpdatePost(Guid postId, PostUpdateDto postUpdateDto, CancellationToken cancellationToken)
        {
            var result = await postService.UpdatePostAsync(postId, postUpdateDto, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Post updated successfully";

            Response.Headers.CacheControl = "no-cache";

            return Accepted(result);

        }
        [HttpGet("{postId:guid}")]
        public async Task<ActionResult> GetPost(Guid postId, CancellationToken cancellationToken)
        {
            var result = await postService.GetPostAsync(postId, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Post retrieved successfully";

            Response.Headers.CacheControl = "no-cache";

            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult> GetPosts(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await postService.GetPostsAsync(pageNumber, pageSize, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Posts retrieved successfully";

            Response.Headers.CacheControl = "no-cache";

            return Ok(result);

        }
    }
}
