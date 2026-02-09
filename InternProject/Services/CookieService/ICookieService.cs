namespace InternProject.Services.CookieService
{
    public interface ICookieService
    {
        void SetRefreshTokenCookie(string token, DateTime expires);
        void DeleteRefreshTokenCookie();
    }
}
