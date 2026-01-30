namespace InternProject.Services
{
    public interface ICookieService
    {
        void SetRefreshTokenCookie(string token, DateTime expires);
        void DeleteRefreshTokenCookie();
    }
}
