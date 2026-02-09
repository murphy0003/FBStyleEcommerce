using System.Security.Claims;

namespace InternProject.Services.UserService
{
    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        public Guid GetCurrentUserId()
        {
            var id = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
        }

        public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
