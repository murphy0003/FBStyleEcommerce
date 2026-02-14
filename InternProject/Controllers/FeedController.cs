using InternProject.Services.FeedService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedController(IFeedService feedService) : ControllerBase
    {
        [HttpGet("global")]
        [OutputCache(Duration = 10, VaryByQueryKeys = new[] { "cursor", "itemName" })]
        public async Task<IActionResult> GetGlobalFeed([FromQuery] string? itemName , [FromQuery] DateTime? cursor , CancellationToken ct , [FromQuery] int pageSize=20)
        {
            var result = await feedService.GetGlobalFeed(itemName, cursor, ct, pageSize);
            return Ok(result);
        }
    }
}
