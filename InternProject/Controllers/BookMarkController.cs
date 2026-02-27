using InternProject.Services.BookMarkService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookMarkController(IBookMarkService bookMarkService) : ControllerBase
    {
        [HttpPost("{postId:guid}")]
        [Authorize]
        public async Task<ActionResult> AddBookMark(
                Guid postId,
                CancellationToken cancellationToken)
        {
            await bookMarkService.AddBookMarkAsync(postId, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Bookmark added successfully";
            Response.Headers.CacheControl = "no-cache";

            return Ok();
        }

        [HttpDelete("{postId:guid}")]
        [Authorize]
        public async Task<ActionResult> RemoveBookMark(
            Guid postId,
            CancellationToken cancellationToken)
        {
            await bookMarkService.RemoveBookMarkAsync(postId, cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Bookmark removed successfully";
            Response.Headers.CacheControl = "no-cache";

            return Ok();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetUserBookMarks(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken cancellationToken = default)
        {
            var result = await bookMarkService.GetBookMarksAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Bookmarks retrieved successfully";
            Response.Headers.CacheControl = "no-cache";

            return Ok(result);
        }
    }
}
