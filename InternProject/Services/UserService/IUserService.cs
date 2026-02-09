using InternProject.Dtos;

namespace InternProject.Services.UserService
{
    public interface IUserService
    {
        Task RegisterV1UserAsync(RegisterV1UserDto dto,CancellationToken cancellationToken);
        Task RegisterV2UserInitAsync(RegisterV2UserInitDto dto,CancellationToken cancellationToken);
        Task RegisterV2UserCompAsync(RegisterV2UserCompDto dto,CancellationToken cancellationToken);
        Task VerifyRegisterEmailAsync(VerifyRegisterEmailDto dto,CancellationToken cancellationToken);
        Task ResendOtpAsync(ResendOtpDto resendOtpDto,CancellationToken cancellationToken);
        Task <(LoginResponseDto?, string? refreshToken)> LoginUserAsync(LoginDto loginDto,CancellationToken cancellationToken);
        Task<(LoginResponseDto?, string? refreshToken)> RefreshTokensAsync(string oldToken,CancellationToken cancellationToken);
        Task ForgetPasswordAsync(ForgetPasswordDto dto,CancellationToken cancellationToken);
        Task VerifyForgetPasswordAsync(VerifyForgetPasswordDto dto,CancellationToken cancellationToken);
        Task ResetPasswordAsync(ResetPasswordDto dto,CancellationToken cancellationToken);
        Task LogoutAsync(string refreshToken,CancellationToken cancellationToken);
    }
}
