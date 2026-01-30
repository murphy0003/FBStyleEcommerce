using System.ComponentModel.DataAnnotations;

namespace InternProject.Dtos
{
    public record RegisterV1UserDto(
        [Required] string UserName,
        [Required, EmailAddress] string Email,
        [Required, StringLength(100, MinimumLength = 6)] string Password,
        [Required] string Phone,
        [Required] string AccountType);
    public record RegisterV2UserInitDto(
        [Required] string UserName,
        [Required, EmailAddress] string Email,
        [Required] string AccountType
        );
    public record RegisterV2UserCompDto(
        string Email,
        [Required, StringLength(100, MinimumLength = 6)] string Password
        );
    public record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
    public record VerifyRegisterEmailDto(string Email, string Otp);
    public record ResendOtpDto(string Email);
    public record LoginResponseDto(string AccessToken,string UserName);
    public record RefreshTokenRequestDto([Required] string RefreshToken);
    public record ForgetPasswordDto (string Email);
    public record VerifyForgetPasswordDto(string Email,string Otp);
    public record ResetPasswordDto(string Email , string NewPassword);
   
}
