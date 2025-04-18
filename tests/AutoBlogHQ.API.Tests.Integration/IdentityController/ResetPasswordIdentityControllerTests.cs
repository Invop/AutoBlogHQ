using System.Net;
using System.Net.Http.Json;
using System.Text;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using AutoBlogHQ.Contracts.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class IdentityControllerResetPasswordTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly AutoBlogApiFactory _factory;

    public IdentityControllerResetPasswordTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResetPassword_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "invalid-email",
            NewPassword: "NewPassword1!",
            ResetCode: "validResetCode"
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "weak", // Doesn't meet requirements
            ResetCode: "validResetCode"
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithNonExistentEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "nonexistent@example.com",
            NewPassword: "NewPassword1!",
            ResetCode: "validResetCode"
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidResetCode_ReturnsBadRequest()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();

        // Use an invalid format for the reset code
        var invalidResetCode = "not-base64-url-encoded";

        var request = new ResetPasswordRequest(
            testUser.Email,
            NewPassword: "NewPassword1!",
            ResetCode: invalidResetCode
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithValidDataButInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();

        // Create a valid format but invalid token
        var fakeToken = "FakeResetToken";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(fakeToken));

        var request = new ResetPasswordRequest(
            testUser.Email,
            NewPassword: "NewPassword1!",
            ResetCode: encodedToken
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(content);
        Assert.NotNull(content.Errors);
        Assert.NotEmpty(content.Errors);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testUser = _factory.TestUsers.First();
        var user = await userManager.FindByEmailAsync(testUser.Email);

        // Generate a real reset token
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

        var request = new ResetPasswordRequest(
            testUser.Email,
            NewPassword: "NewValidPassword1!",
            ResetCode: encodedResetToken
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);
        var content = await response.Content.ReadFromJsonAsync<SuccessResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Password has been reset successfully", content.Message);

        // Verify the password was actually changed
        var loginRequest = new LoginRequest(testUser.Email, "NewValidPassword1!", true);
        var loginResponse = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.Login, loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Reset the password back to the original for other tests
        var currentToken = await userManager.GeneratePasswordResetTokenAsync(user);
        await userManager.ResetPasswordAsync(user, currentToken, testUser.Password);
    }

    [Fact]
    public async Task ResetPassword_WithUnconfirmedEmail_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create a user with unconfirmed email
        var email = "unconfirmed@example.com";
        var unconfirmedUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = false
        };

        await userManager.CreateAsync(unconfirmedUser, "Password123!");

        // Generate a token (which won't be accepted due to unconfirmed email)
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(unconfirmedUser);
        var encodedResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

        var request = new ResetPasswordRequest(
            email,
            NewPassword: "NewPassword1!",
            ResetCode: encodedResetToken
        );

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ResetPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Clean up
        await userManager.DeleteAsync(unconfirmedUser);
    }
}