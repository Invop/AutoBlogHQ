using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class ForgotPasswordIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly AutoBlogApiFactory _factory;
    private readonly MockEmailSender _mockEmailSender;

    public ForgotPasswordIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _mockEmailSender = (MockEmailSender)_factory.Services.GetRequiredService<IEmailSender>();
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequest("invalid-email");

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ForgotPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ReturnsAccepted()
    {
        // Arrange
        var request = new ForgotPasswordRequest("nonexistent@example.com");

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ForgotPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(_mockEmailSender.SentEmails);
    }

    [Fact]
    public async Task ForgotPassword_WithExistingConfirmedEmail_ReturnsAcceptedAndSendsEmail()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var request = new ForgotPasswordRequest(testUser.Email);

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ForgotPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var sentEmail = Assert.Single(_mockEmailSender.SentEmails);
        Assert.Equal(testUser.Email, sentEmail.Recipient);
        Assert.Equal("Your password reset code", sentEmail.Subject);
        Assert.NotEmpty(sentEmail.Content);
    }

    [Fact]
    public async Task ForgotPassword_WithExistingUnconfirmedEmail_ReturnsAcceptedWithoutSendingEmail()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create an unconfirmed user
        var unconfirmedUser = new ApplicationUser
        {
            Email = "unconfirmed@example.com",
            UserName = "unconfirmed@example.com",
            EmailConfirmed = false
        };

        await userManager.CreateAsync(unconfirmedUser, "Password123!");

        // Clear any emails sent during setup
        _mockEmailSender.SentEmails.Clear();

        var request = new ForgotPasswordRequest(unconfirmedUser.Email);

        // Act
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.ForgotPassword, request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(_mockEmailSender.SentEmails);

        // Clean up
        await userManager.DeleteAsync(unconfirmedUser);
    }
}