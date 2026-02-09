using InternProject.Models.UserModels;

namespace InternProject.Services.TokenService
{
    public interface ITokenService
    {
        string CreateToken(Users user);
        string GenerateRefreshToken();
    }
}
