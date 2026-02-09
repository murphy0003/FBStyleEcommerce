using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Models.UserModels;
using InternProject.Services.EmailService;
using InternProject.Services.TokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace InternProject.Services.UserService
{
    public class UserService(AppDbContext context,IEmailService emailService,ITokenService tokenService) : IUserService
    {
        public async Task RegisterV1UserAsync(RegisterV1UserDto dto, CancellationToken cancellationToken)
        {
            var userExists = await context.Users
            .AnyAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase),cancellationToken);
            if (userExists)
            {
                throw new ApiException(
                    "USER_ALREADY_EXISTS",
                    new
                    {
                        email = "User with this email already exists"
                    },
                    StatusCodes.Status409Conflict);
            }
            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                string otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var newUser = UserMappings.ToV1Model(dto);
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                newUser.VerificationOtp = otpCode;
                newUser.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
                await context.Users.AddAsync(newUser,cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                await emailService.SendOtpEmailAsync(newUser.Email, otpCode,cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        public async Task RegisterV2UserInitAsync(RegisterV2UserInitDto dto, CancellationToken cancellationToken)
        {
            var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (existingUser != null)
            {
                if (existingUser.IsRegistrationCompleted)
                {
                    throw new ApiException(
                        "USER_ALREADY_EXISTS",
                        new { email = "User already registered" },
                        StatusCodes.Status409Conflict);
                }

                if (existingUser.RegistrationExpiresAt > DateTime.UtcNow)
                {
                    throw new ApiException(
                        "REGISTRATION_IN_PROGRESS",
                        new { retryAfterSeconds = (int)(existingUser.RegistrationExpiresAt.Value - DateTime.UtcNow).TotalSeconds },
                        StatusCodes.Status409Conflict);
                }

                // expired → cleanup
                context.Users.Remove(existingUser);
                await context.SaveChangesAsync(cancellationToken);
            }
            string otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var newUser = UserMappings.ToV2Model(dto);
            newUser.VerificationOtp = otpCode;
            newUser.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
            newUser.Password = string.Empty;
            newUser.IsEmailVerified = false;
            newUser.IsRegistrationCompleted = false;
            newUser.RegistrationExpiresAt = DateTime.UtcNow.AddMinutes(30);
            await context.Users.AddAsync(newUser, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await emailService.SendOtpEmailAsync(newUser.Email, otpCode, cancellationToken);
        }
        public async Task RegisterV2UserCompAsync(RegisterV2UserCompDto dto, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (user == null ||
                user.IsEmailVerified != true ||
                user.IsRegistrationCompleted ||
                user.RegistrationExpiresAt < DateTime.UtcNow)
            {
                await Task.Delay(Random.Shared.Next(5, 30), cancellationToken);

                throw new ApiException(
                    "REGISTRATION_NOT_ALLOWED",
                    null,
                    StatusCodes.Status400BadRequest);
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.Phone = String.Empty;
            user.IsRegistrationCompleted = true;
            user.RegistrationExpiresAt = null;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(LoginResponseDto?, string? refreshToken)> LoginUserAsync(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(loginDto.Email, StringComparison.CurrentCultureIgnoreCase), cancellationToken);

            var fakeHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgNo8z6ucl2j6H59zKk.58L.p0qK";

            var hashToVerify = user?.Password ?? fakeHash;
            bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, hashToVerify);

            if (!isValid || user is null)
            {
                if (user is not null)
                {
                    user.AccessFailedCount++;
                    if (user.AccessFailedCount >= 5)
                        user.LockoutEnd = DateTime.UtcNow.AddHours(10);
                    await context.SaveChangesAsync(cancellationToken);
                }
                await Task.Delay(Random.Shared.Next(5, 10), cancellationToken);
                throw new ApiException(
                      "INVALID_CREDENTIALS",
                      null,
                      StatusCodes.Status401Unauthorized
                );
            }
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                throw new ApiException(
                    "ACCOUNT_LOCKED",
                    new { lockedUntil = user.LockoutEnd },
                    StatusCodes.Status423Locked
                );
            if (user.Status != AccountStatus.Active)
                throw new ApiException(
                    "ACCOUNT_NOT_ACTIVE",
                    null,
                    StatusCodes.Status403Forbidden
                );
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;

            var refreshToken = tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await context.SaveChangesAsync(cancellationToken);

            var response = await CreateTokenResponse(user);

            return (response, refreshToken);
        }

        private async Task<LoginResponseDto?> CreateTokenResponse(Users? user)
        {
            return new LoginResponseDto
                        (
                            tokenService.CreateToken(user),
                            user.UserName
                        );
        }

        public async Task<(LoginResponseDto?, string? refreshToken)> RefreshTokensAsync(string oldToken,CancellationToken cancellationToken)
        {
            var user = await ValidateRefreshTokenAsync(oldToken,cancellationToken) ?? throw new ApiException(
                      "INVALID_REFRESH_TOKEN",
                      null,
                      StatusCodes.Status401Unauthorized
                );
            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new ApiException(
                    "REFRESH_TOKEN_EXPIRED",
                    null,
                    StatusCodes.Status401Unauthorized
                );
            await RevokeRefreshTokenAsync(oldToken,cancellationToken);
            var newRefreshToken = tokenService.GenerateRefreshToken();
            user?.RefreshToken = newRefreshToken;
            user?.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync(cancellationToken);
            var response = await CreateTokenResponse(user);
            return (response, newRefreshToken);
        }
       
        private async Task RevokeRefreshTokenAsync(string refreshToken,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken,cancellationToken);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
           


        private async Task<Users?> ValidateRefreshTokenAsync(string refreshToken,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u=>u.RefreshToken==refreshToken,cancellationToken);
            if(user is null 
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null; 
            }
            return user;
        }

        public async Task ResendOtpAsync(ResendOtpDto resendOtpDto,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u=>u.Email.Equals(resendOtpDto.Email, StringComparison.CurrentCultureIgnoreCase),cancellationToken)
                ?? throw new ApiException(
                    "USER_NOT_FOUND",
                    new { email = resendOtpDto.Email },
                    StatusCodes.Status404NotFound
                );
            if (user.LastOtpSentAt.HasValue)
            {
                var secondsPassed =
                    (DateTime.UtcNow - user.LastOtpSentAt.Value).TotalSeconds;

                if (secondsPassed < 60)
                {
                    var secondsRemaining = 60 - (int)secondsPassed;

                    throw new ApiException(
                        "OTP_RATE_LIMIT",
                        new { retryAfterSeconds = secondsRemaining },
                        StatusCodes.Status429TooManyRequests
                    );
                }
            }
            string newOtp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.VerificationOtp = newOtp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.LastOtpSentAt = DateTime.UtcNow;
            user.VerificationAttempts = 0;

            await context.SaveChangesAsync(cancellationToken);
            await emailService.SendOtpEmailAsync(user.Email, newOtp,cancellationToken);
        }

        public async Task VerifyRegisterEmailAsync(VerifyRegisterEmailDto dto,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase), cancellationToken) ?? throw new ApiException(
            "USER_NOT_FOUND",
            new { email = dto.Email },
            StatusCodes.Status404NotFound
        );
            if (user.IsEmailVerified)
                throw new ApiException(
                    "EMAIL_ALREADY_VERIFIED",
                    null,
                    StatusCodes.Status409Conflict
                );

            if (user.OtpExpiry < DateTime.UtcNow)
                throw new ApiException(
                    "OTP_EXPIRED",
                    null,
                    StatusCodes.Status410Gone
                );

            if (user.VerificationAttempts >= 5)
                throw new ApiException(
                    "ACCOUNT_LOCKED",
                    "Too many failed attempts",
                    StatusCodes.Status423Locked
                );

            if (user.VerificationOtp != dto.Otp)
            {
                user.VerificationAttempts++;
                await context.SaveChangesAsync(cancellationToken);

                throw new ApiException(
                    "INVALID_OTP",
                    null,
                    StatusCodes.Status400BadRequest
                );
            }
            user.IsEmailVerified = true;
            user.VerificationOtp = null;
            user.Status = AccountStatus.Active;
            user.VerificationAttempts = 0;
            user.OtpExpiry = null;

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ForgetPasswordAsync(ForgetPasswordDto dto,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u=>u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase),cancellationToken);
            if (user == null || user.Status != AccountStatus.Active)
            {
                await Task.Delay(Random.Shared.Next(5, 30),cancellationToken);
                return;
            }
            
                string otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                user.VerificationOtp = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
                user.VerificationAttempts = 0;
                user.IsPasswordResetVerified = false;
                await context.SaveChangesAsync(cancellationToken);
                await emailService.SendOtpEmailAsync(user.Email, otp,cancellationToken);
                
        }

        public async Task VerifyForgetPasswordAsync(VerifyForgetPasswordDto dto,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase),cancellationToken);
            if (user == null)
            {
                await Task.Delay(Random.Shared.Next(5, 30), cancellationToken);
                throw new ApiException(
                    "INVALID_OR_EXPIRED_OTP",
                    null,
                    StatusCodes.Status400BadRequest
                );
            }
            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
                throw new ApiException(
                    "INVALID_OR_EXPIRED_OTP",
                    null,
                    StatusCodes.Status400BadRequest
                );


            if (user.VerificationAttempts >= 5)
                throw new ApiException(
                    "OTP_ATTEMPTS_EXCEEDED",
                    null,
                    StatusCodes.Status423Locked
                );


            if (user.VerificationOtp != dto.Otp)
            {
                user.VerificationAttempts++;
                await context.SaveChangesAsync(cancellationToken);
                throw new ApiException(
                        "INVALID_OR_EXPIRED_OTP",
                        null,
                        StatusCodes.Status400BadRequest
                );
            }
            
            user.IsPasswordResetVerified = true;
            user.VerificationAttempts = 0;
            user.VerificationOtp = null;
            user.OtpExpiry = null;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto,CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(dto.Email, StringComparison.CurrentCultureIgnoreCase),cancellationToken);
            if (user == null || user.IsPasswordResetVerified is not true)
            {
                await Task.Delay(Random.Shared.Next(5, 30), cancellationToken);

                throw new ApiException(
                    "PASSWORD_RESET_NOT_ALLOWED",
                    null,
                    StatusCodes.Status400BadRequest
                );
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsPasswordResetVerified = false;
            await context.SaveChangesAsync(cancellationToken);
        }
        public async Task LogoutAsync(string refreshToken,CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;
            var user = await ValidateRefreshTokenAsync(refreshToken, cancellationToken);
            if (user == null)
                return;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
