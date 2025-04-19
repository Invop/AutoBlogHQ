using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using AutoBlogHQ.Contracts.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController
{
    public class ChangePasswordIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
    {
        private readonly HttpClient _client;
        private readonly AutoBlogApiFactory _factory;

        public ChangePasswordIdentityControllerTests(AutoBlogApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private ChangePasswordRequest CreateRequest(
            string oldPassword = "OldPassword1!",
            string newPassword = "NewPassword1!",
            string confirmPassword = "NewPassword1!")
        {
            return new ChangePasswordRequest(
                OldPassword: oldPassword,
                NewPassword: newPassword,
                ConfirmNewPassword: confirmPassword);
        }

        [Fact]
        public async Task ChangePassword_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var authClient = await _factory.GetAuthenticatedClientAsync(
                testUser.UserName, 
                testUser.Password);
            
            var request = CreateRequest(
                oldPassword: testUser.Password,
                newPassword: "NewValidPassword1!",
                confirmPassword: "NewValidPassword1!");

            // Act
            var response = await authClient.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify password was actually changed
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(testUser.Email);
            var passwordValid = await userManager.CheckPasswordAsync(user, "NewValidPassword1!");
            Assert.True(passwordValid);
        }

        [Fact]
        public async Task ChangePassword_WhenNotAuthenticated_ReturnsNotFound()
        {
            // Arrange
            var request = CreateRequest();

            // Act
            var response = await _client.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidOldPassword_ReturnsBadRequest()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var authClient = await _factory.GetAuthenticatedClientAsync(
                testUser.UserName, 
                testUser.Password);
            
            var request = CreateRequest(
                oldPassword: "WrongOldPassword1!");

            // Act
            var response = await authClient.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("Change password failed", content.Message);
        }

        [Fact]
        public async Task ChangePassword_WithMismatchedNewPasswords_ReturnsBadRequest()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var authClient = await _factory.GetAuthenticatedClientAsync(
                testUser.UserName, 
                testUser.Password);
            
            var request = CreateRequest(
                newPassword: "NewPassword1!",
                confirmPassword: "DifferentPassword1!");

            // Act
            var response = await authClient.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("short", "NewPassword1!", "NewPassword1!")] // Invalid new password
        [InlineData("OldPassword1!", "nouppercase1!", "nouppercase1!")] // No uppercase
        [InlineData("OldPassword1!", "NoNumber!", "NoNumber!")] // No number
        public async Task ChangePassword_WithInvalidNewPassword_ReturnsBadRequest(
            string oldPassword, string newPassword, string confirmPassword)
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var authClient = await _factory.GetAuthenticatedClientAsync(
                testUser.UserName, 
                testUser.Password);
            
            var request = CreateRequest(
                oldPassword: oldPassword,
                newPassword: newPassword,
                confirmPassword: confirmPassword);

            // Act
            var response = await authClient.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithEmptyRequest_ReturnsBadRequest()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var authClient = await _factory.GetAuthenticatedClientAsync(
                testUser.UserName, 
                testUser.Password);
            
            var request = new ChangePasswordRequest("", "", "");

            // Act
            var response = await authClient.PutAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ChangePassword,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}