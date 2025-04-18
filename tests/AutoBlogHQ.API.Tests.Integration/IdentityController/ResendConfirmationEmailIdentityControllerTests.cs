using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class ResendConfirmationEmailControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly MockEmailSender? _emailSender;
    private readonly AutoBlogApiFactory _factory;

    public ResendConfirmationEmailControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _emailSender = factory.Services.GetRequiredService<IEmailSender>() as MockEmailSender;
    }

    private ResendConfirmationEmailRequest CreateRequest(string email = "")
    {
        return new ResendConfirmationEmailRequest(email);
    }

    [Fact]
    public async Task ResendConfirmationEmail_WithValidEmail_ReturnsAccepted()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
        // Create new unconfirmed test user
        var newUser = new ApplicationUser
        {
            UserName = $"testuser_{Guid.NewGuid()}",
            Email = $"test_{Guid.NewGuid()}@example.com",
            EmailConfirmed = false
        };
    
        var createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            throw new Exception($"Failed to create test user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        var request = CreateRequest(newUser.Email);
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Single(_emailSender.SentEmails);
        var sentEmail = _emailSender.SentEmails[0];
        Assert.Equal(newUser.Email, sentEmail.Recipient);
        Assert.Equal("Confirm your email", sentEmail.Subject);

        // Cleanup
        await userManager.DeleteAsync(newUser);
    }

    [Fact]
    public async Task ResendConfirmationEmail_WithNonExistentEmail_ReturnsAccepted()
    {
        // Arrange
        var request = CreateRequest("nonexistent@example.com");
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task ResendConfirmationEmail_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateRequest();
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task ResendConfirmationEmail_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateRequest("invalid-email");
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task ResendConfirmationEmail_ForAlreadyConfirmedEmail_ReturnsAccepted()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var request = CreateRequest(testUser.Email);
        _emailSender.SentEmails.Clear();


        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }
}