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

public class ConfirmEmailIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly AutoBlogApiFactory _factory;

    public ConfirmEmailIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<ConfirmEmailRequest> CreateValidRequest(string userId = null, string code = null)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testUser = _factory.TestUsers.First();
        var user = await userManager.FindByEmailAsync(testUser.Email);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return new ConfirmEmailRequest(
            userId ?? user.Id,
            code ?? encodedToken
        );
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ReturnsOk()
    {
        // Arrange
        var request = await CreateValidRequest();

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={request.UserId}&code={request.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<SuccessResponse>();
        Assert.Equal("Thank you for confirming your email!", content.Message);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = await CreateValidRequest();
        var invalidRequest = request with { UserId = "invalid-user-id" };

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={invalidRequest.UserId}&code={invalidRequest.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidTokenFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = await CreateValidRequest();
        var invalidRequest = request with { Code = "invalid-token-format" };

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={invalidRequest.UserId}&code={invalidRequest.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testUser = _factory.TestUsers.First();
        var user = await userManager.FindByEmailAsync(testUser.Email);

        // Generate and expire token by changing security stamp
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var request = new ConfirmEmailRequest(user.Id, encodedToken);

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={request.UserId}&code={request.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("Email confirmation failed", content.Message);
    }

    [Fact]
    public async Task ConfirmEmail_WithEmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var request = await CreateValidRequest();
        var invalidRequest = request with { UserId = "" };

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={invalidRequest.UserId}&code={invalidRequest.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var request = await CreateValidRequest();
        var invalidRequest = request with { Code = "" };

        // Act
        var response = await _client.GetAsync(
            $"{ApiEndpoints.IdentityEndpoints.ConfirmEmail}?userId={invalidRequest.UserId}&code={invalidRequest.Code}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}