using InternProject.Models;

namespace InternProject.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string GenerateRefreshToken();
    }
}
