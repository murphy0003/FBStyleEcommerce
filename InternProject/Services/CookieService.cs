
using Microsoft.AspNetCore.Http;

namespace InternProject.Services
{
    public class CookieService(IHttpContextAccessor httpContextAccessor) : ICookieService
    {
        public void DeleteRefreshTokenCookie()
        {
            httpContextAccessor.HttpContext!.Response.Cookies.Append(
                "refreshToken",
                "",
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
        }

        public void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,   
                Secure = true,    
                SameSite = SameSiteMode.Strict,
                Expires = expires
            };

            httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
