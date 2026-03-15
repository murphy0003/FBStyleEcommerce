using InternProject.Data;
using InternProject.Dtos;
using InternProject.Models.ApiModels;
using InternProject.Models.UserModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace InternProject.Tests
{
    public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly CookieContainer _cookieContainer = new();
        public UserIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            });
        }
        [Fact]
        public async Task Complete_Registration_V1_Flow()
        {
            var registration = new RegisterV1UserDto
            (
                 "flowtester",
                 "flow_test@example.com",
                 "Password123!",
                 "FlowTester",
                 "Seller"
            );

            var regResponse = await _client.PostAsJsonAsync("/api/Auth/register/v1", registration);

            Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == registration.Email);

            Assert.NotNull(user);
            Assert.False(user.IsEmailVerified);
        }
        [Fact]
        public async Task RegisterV2_Complete_ShouldReturnOk_WhenValid()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Users.Add(new User
                {
                    UserName = "Test",
                    Email = "v2test@test.com",
                    IsEmailVerified = true,
                    IsRegistrationCompleted = false,
                    RegistrationExpiresAt = DateTime.UtcNow.AddMinutes(30)
                });
                await db.SaveChangesAsync();
            }

            var dto = new RegisterV2UserCompDto ("v2test@test.com" , "NewPassword123!" );

            var response = await _client.PatchAsJsonAsync("/api/auth/register/v2/complete", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task Login_ShouldReturnToken_AndSetCookie()
        {
            var password = "Password123!";

            var loginDto = new LoginDto ("test@example.com", password );

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(response.Headers.Contains("Set-Cookie"), "Response should contain a Set-Cookie header");
            var cookieHeader = response.Headers.GetValues("Set-Cookie").First();
            Assert.Contains("refreshToken", cookieHeader);
        }
        [Fact]
        public async Task ForgetPassword_ShouldAlwaysReturnOk_ToPreventEnumeration()
        {
           
            var dto = new ForgetPasswordDto ( "not-in-db@test.com" );
            var response = await _client.PostAsJsonAsync("/api/auth/forget-password", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task Full_Auth_Lifecycle_Test()
        {
            var loginDto = new LoginDto ( "test@example.com" , "Password123!" );
            var loginRes = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
            var setCookieHeaders = loginRes.Headers.TryGetValues("Set-Cookie", out var values);
            Assert.True(setCookieHeaders);

            var hasRefreshToken = values!.Any(v => v.Contains("refreshToken"));
            Assert.True(hasRefreshToken, "The refreshToken cookie was never received!");

            var refreshRes = await _client.PostAsync("/api/auth/refresh-token", null);
            var body = await refreshRes.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);
            var response = await refreshRes.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();

            Assert.NotNull(response);
            Assert.True(response.Status);
            Assert.NotNull(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Data.AccessToken));
        }
        [Fact]
        public async Task Logout_ShouldSetSecurityHeaders()
        {
            var loginDto = new LoginDto("test@example.com", "Password123!");
            var loginRes = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var authData = await loginRes.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData!.Data!.AccessToken);
            var response = await _client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("no-store, must-revalidate, no-cache", response.Headers.CacheControl?.ToString());
            Assert.Equal("0", response.Content.Headers.GetValues("Expires").First());
            Assert.Equal("no-cache", response.Headers.GetValues("Pragma").First());
        }
    }
}
