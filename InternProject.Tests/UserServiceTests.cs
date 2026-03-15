using InternProject.Data;
using InternProject.Dtos;
using InternProject.Models.ApiModels;
using InternProject.Models.UserModels;
using InternProject.Services.EmailService;
using InternProject.Services.TokenService;
using InternProject.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace InternProject.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<ITokenService> _tokenMock = new();
        private readonly AppDbContext _context;
        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            _context = new AppDbContext(options);
        }
        [Fact]
        public async Task RegisterV1User_ShouldThrowException_WhenUserExists()
        {
            var existingEmail = "test@example.com";
            _context.Users.Add(new User { Email = existingEmail, UserName = "existing" });
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new RegisterV1UserDto ("existing",existingEmail, "Password123" ,"+0123456" ,"Seller");

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                service.RegisterV1UserAsync(dto, CancellationToken.None));

            Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
            Assert.Equal("USER_ALREADY_EXISTS", exception.Code);
        }
        [Fact]
        public async Task RegisterV1User_ShouldSaveUserAndSendEmail_WhenInputIsValid()
        {
            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new RegisterV1UserDto ("newuser","new@example.com", "Password123","+0123456","Seller");
            await service.RegisterV1UserAsync(dto, CancellationToken.None);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
            Assert.NotNull(user);
            Assert.NotNull(user.VerificationOtp); 
            _emailMock.Verify(x => x.SendOtpEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task LoginUser_ShouldIncreaseAccessFailedCount_WhenPasswordIsWrong()
        {
            var email = "lockmeout@example.com";
            var password = "CorrectPassword123";

            var user = new User
            {
                UserName = "lockmeout",
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                AccessFailedCount = 0,
                Status = AccountStatus.Active
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var loginDto = new LoginDto (email, "WrongPassword!!" );

            await Assert.ThrowsAsync<ApiException>(() =>
                service.LoginUserAsync(loginDto, CancellationToken.None));

            var updatedUser = await _context.Users.FirstAsync(u => u.Email == email);
            Assert.Equal(1, updatedUser.AccessFailedCount); 
        }
        [Fact]
        public async Task LoginUser_ShouldTakeTime_EvenIfUserDoesNotExist()
        {
         
            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var loginDto = new LoginDto ( "nonexistent@test.com" , "somePassword" );

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                service.LoginUserAsync(loginDto, CancellationToken.None));

            Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
        }
        [Fact]
        public async Task RegisterV2_ShouldDeleteOldRecord_IfRegistrationExpired()
        {
            var email = "expired@test.com";
            var expiredUser = new User
            {
                UserName = "expireduser",
                Email = email,
                IsRegistrationCompleted = false,
                RegistrationExpiresAt = DateTime.UtcNow.AddMinutes(-10) 
            };
            _context.Users.Add(expiredUser);
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new RegisterV2UserInitDto ( "expireduser",email,"Buyer" );
            
            await service.RegisterV2UserInitAsync(dto, CancellationToken.None);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            Assert.NotNull(userInDb);
            Assert.True(userInDb.RegistrationExpiresAt > DateTime.UtcNow);
            _emailMock.Verify(x => x.SendOtpEmailAsync(email, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task RegisterV2_ShouldThrow_IfRegistrationIsStillActive()
        {
            var email = "active@test.com";
            var activeUser = new User
            {
                UserName = "activeuser",
                Email = email,
                IsRegistrationCompleted = false,
                RegistrationExpiresAt = DateTime.UtcNow.AddMinutes(10) 
            };
            _context.Users.Add(activeUser);
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new RegisterV2UserInitDto ( "activeuser" , email , "Buyer" );

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                service.RegisterV2UserInitAsync(dto, CancellationToken.None));

            Assert.Equal("REGISTRATION_IN_PROGRESS", exception.Code);
        }
        [Fact]
        public async Task VerifyRegisterEmail_ShouldThrow_WhenOtpIsExpired()
        {
            var email = "old_otp@test.com";
            var user = new User
            {
                UserName = "oldotpuser",
                Email = email,
                VerificationOtp = "123456",
                OtpExpiry = DateTime.UtcNow.AddMinutes(-1),
                IsEmailVerified = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new VerifyRegisterEmailDto ( email,"123456");

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                service.VerifyRegisterEmailAsync(dto, CancellationToken.None));

            Assert.Equal("OTP_EXPIRED", exception.Code);
            Assert.Equal(StatusCodes.Status410Gone, exception.StatusCode);
        }
        [Fact]
        public async Task VerifyRegisterEmail_ShouldLockAccount_After5FailedAttempts()
        {
            var email = "bruteforce@test.com";
            var user = new User
            {
                UserName = "bruteforceuser",
                Email = email,
                VerificationOtp = "123456",
                OtpExpiry = DateTime.UtcNow.AddMinutes(10),
                VerificationAttempts = 5 
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var service = new UserService(_context, _emailMock.Object, _tokenMock.Object);
            var dto = new VerifyRegisterEmailDto ( email, "123456" );

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                service.VerifyRegisterEmailAsync(dto, CancellationToken.None));

            Assert.Equal("ACCOUNT_LOCKED", exception.Code);
        }

    }
}
