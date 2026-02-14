using InternProject.Dtos;
using InternProject.Services.ProfileService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetOrCreateProfile(CancellationToken cancellationToken)
        {
            var result = await profileService.GetOrCreateProfile(cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Profile retrieved successfully";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
        [HttpPatch("{profileId:guid})")]
        public async Task<ActionResult> UpdateProfile(Guid profileId,[FromBody] UpdateProfileRequestDto request, CancellationToken cancellationToken)
        {
            var result = await profileService.UpdateProfile(profileId,request, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Profile updated successfully";
            Response.Headers.CacheControl = "no-cache";
            return Accepted(result);
        }
    }
}
