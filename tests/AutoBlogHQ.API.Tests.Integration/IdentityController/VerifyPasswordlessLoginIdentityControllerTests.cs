using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController
{
    public class VerifyPasswordlessLoginIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
    {
        private readonly HttpClient _client;
        private readonly AutoBlogApiFactory _factory;

        public VerifyPasswordlessLoginIdentityControllerTests(AutoBlogApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task VerifyPasswordlessLogin_WithValidCode_ReturnsOk()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var testUser = _factory.TestUsers.First();
            var user = await userManager.FindByEmailAsync(testUser.Email);
            
            var code = await userManager.GenerateUserTokenAsync(
                user, 
                "PasswordlessLoginTotpProvider", 
                "passwordless-auth");

            var request = new VerifyPasswordlessLoginRequest(
                Email: testUser.Email,
                Code: code,
                RememberMe: true);

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.VerifyPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task VerifyPasswordlessLogin_WithInvalidCode_ReturnsUnauthorized()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var request = new VerifyPasswordlessLoginRequest(
                Email: testUser.Email,
                Code: "invalid-code",
                RememberMe: true);

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.VerifyPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task VerifyPasswordlessLogin_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var request = new VerifyPasswordlessLoginRequest(
                Email: "nonexistent@example.com",
                Code: "any-code",
                RememberMe: true);

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.VerifyPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task VerifyPasswordlessLogin_WithEmptyCode_ReturnsBadRequest()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var request = new VerifyPasswordlessLoginRequest(
                Email: testUser.Email,
                Code: "",
                RememberMe: true);

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.VerifyPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}