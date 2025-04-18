using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class PasswordlessLoginIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly MockEmailSender _emailSender;
    private readonly AutoBlogApiFactory _factory;

    public PasswordlessLoginIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _emailSender = (MockEmailSender)factory.Services.GetRequiredService<IEmailSender>();
    }

    [Fact]
    public async Task PasswordlessLogin_WithValidEmail_ReturnsOkAndSendsCode()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var request = new PasswordlessLoginRequest(testUser.Email);
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.PasswordlessLogin,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(_emailSender.SentEmails);
        var sentEmail = _emailSender.SentEmails[0];
        Assert.Equal(testUser.Email, sentEmail.Recipient);
        Assert.Equal("Your login code", sentEmail.Subject);
    }

    [Fact]
    public async Task PasswordlessLogin_WithNonExistentEmail_ReturnsOkButNoEmail()
    {
        // Arrange
        var request = new PasswordlessLoginRequest("nonexistent@example.com");
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.PasswordlessLogin,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task PasswordlessLogin_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new PasswordlessLoginRequest("invalid-email");
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.PasswordlessLogin,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task PasswordlessLogin_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new PasswordlessLoginRequest("");
        _emailSender.SentEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.PasswordlessLogin,
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_emailSender.SentEmails);
    }
}