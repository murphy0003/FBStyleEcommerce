using InternProject.Models.UserModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InternProject.Services.TokenService
{
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        public string CreateToken(Users user)
        {
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Role, user.Type.ToString())
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Token")!));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer:configuration.GetValue<string>("Jwt:Issuer"),
                audience:configuration.GetValue<string>("Jwt:Audience"),
                claims:claims,
                expires:DateTime.UtcNow.AddHours(1),
                signingCredentials:creds
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
