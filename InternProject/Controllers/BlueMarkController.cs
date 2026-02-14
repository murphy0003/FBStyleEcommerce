using InternProject.Dtos;
using InternProject.Services.BlueMarkService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlueMarkController(IBlueMarkService blueMarkService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> CreateBlueMark(BlueMarkCreateDto blueMarkCreateDto, CancellationToken cancellationToken)
        {
            var result = await blueMarkService.CreateBlueMarkAsync(blueMarkCreateDto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Blue mark created successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
        [HttpPatch]
        public async Task<ActionResult> UpdateBlueMark(BlueMarkUpdateDto blueMarkUpdateDto, CancellationToken cancellationToken)
        {
            var result = await blueMarkService.UpdateBlueMarkAsync(blueMarkUpdateDto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Blue mark updated successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);

        }
    }
}
