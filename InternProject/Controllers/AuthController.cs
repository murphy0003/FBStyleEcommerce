using InternProject.Dtos;
using InternProject.Models.ApiModels;
using InternProject.Services.CookieService;
using InternProject.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUserService userService, ICookieService cookieService) : ControllerBase
    {
        [HttpPost("register/v1")]
        public async Task<ActionResult> RegisterV1User(RegisterV1UserDto dto,CancellationToken cancellationToken)
        {
            await userService.RegisterV1UserAsync(dto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Registration successful.OTP sent to email.";
            return Created("", new { email = dto.Email , otpExpiresInSeconds = 600 });
        }
        [HttpPost("register/v2/init")]
        public async Task <ActionResult> RegisterV2UserInit(RegisterV2UserInitDto dto,CancellationToken cancellationToken)
        {
            await userService.RegisterV2UserInitAsync(dto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Registration started.OTP sent to email";
            return Created("", new { email = dto.Email , otpExpiresInSeconds = 900 } );
        }
        [HttpPatch("register/v2/complete")]
        public async Task <ActionResult> RegisterV2UserComp(RegisterV2UserCompDto dto,CancellationToken cancellationToken)
        {
            await userService.RegisterV2UserCompAsync(dto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Registeration Completed.";
            return Ok();
        }
        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail(VerifyRegisterEmailDto dto, CancellationToken cancellationToken)
        {
            await userService.VerifyRegisterEmailAsync(dto,cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Email verified successfully. You can now login.";
            return Ok();
        }
        [HttpPost("resend-otp")]
        public async Task<ActionResult> ResendOtp(ResendOtpDto dto,CancellationToken cancellationToken)
        {
            await userService.ResendOtpAsync(dto,cancellationToken);
            HttpContext.Items["ResponseMessage"] = "A new OTP has been sent to your email.";
            return Ok(new { email=dto.Email , otpExpiresInSeconds = 600 });
        }
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto,CancellationToken cancellationToken)
        {
            
            
        var (loginResponse, refreshToken) = await userService.LoginUserAsync(loginDto,cancellationToken);
        cookieService.SetRefreshTokenCookie(refreshToken!, DateTime.UtcNow.AddDays(7));
        HttpContext.Items["ResponseMessage"] = "Login successful";
        return Ok(loginResponse);
           
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var oldToken))
                throw new ApiException(
                      "REFRESH_TOKEN_MISSING",
                      null,
                      StatusCodes.Status401Unauthorized
        );
            var (loginResponse, refreshToken) = await userService.RefreshTokensAsync(oldToken,cancellationToken);
            cookieService.SetRefreshTokenCookie(refreshToken!, DateTime.UtcNow.AddDays(7));
            HttpContext.Items["ResponseMessage"] = "Token refreshed";
            return Ok(loginResponse);
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout(CancellationToken cancellationToken)
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                await userService.LogoutAsync(refreshToken, cancellationToken);
            }

            cookieService.DeleteRefreshTokenCookie();
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
            HttpContext.Items["ResponseMessage"] = "Logged out successfully";

            return Ok();
        }
        [HttpPost("forget-password")]
        public async Task<ActionResult> ForgetPassword(ForgetPasswordDto dto,CancellationToken cancellationToken)
        {
            await userService.ForgetPasswordAsync(dto, cancellationToken);
            HttpContext.Items["ResponseMessaage"] = "If the email exists, an OTP has been sent.";
            return Ok(new { email = dto.Email });
        }
        [HttpPost("verify-forget-password")]
        public async Task<ActionResult> VerifyForgetPassword(VerifyForgetPasswordDto dto, CancellationToken cancellationToken)
        {
            await userService.VerifyForgetPasswordAsync(dto,cancellationToken);
            HttpContext.Items["ResponseMessage"] = "OTP verified successfully";
            return Ok(new { email = dto.Email });
        }
        [HttpPost("reset-password")]
        public async Task <ActionResult> ResetPassword(ResetPasswordDto dto,CancellationToken cancellationToken)
        {
            await userService.ResetPasswordAsync(dto,cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Password reset successful";
            return Ok();
        }
    }
}

