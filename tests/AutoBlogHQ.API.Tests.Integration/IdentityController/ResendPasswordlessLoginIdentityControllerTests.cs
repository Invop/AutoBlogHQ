using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController
{
    public class ResendPasswordlessLoginIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
    {
        private readonly HttpClient _client;
        private readonly AutoBlogApiFactory _factory;
        private readonly MockEmailSender _emailSender;

        public ResendPasswordlessLoginIdentityControllerTests(AutoBlogApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _emailSender = (MockEmailSender)factory.Services.GetRequiredService<IEmailSender>();
        }

        [Fact]
        public async Task ResendPasswordlessLogin_WithValidEmail_ReturnsOkAndSendsCode()
        {
            // Arrange
            var testUser = _factory.TestUsers.First();
            var request = new ResendPasswordlessLoginRequest(Email: testUser.Email);
            _emailSender.SentEmails.Clear();

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ResendPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(_emailSender.SentEmails);
            var sentEmail = _emailSender.SentEmails[0];
            Assert.Equal(testUser.Email, sentEmail.Recipient);
            Assert.Equal("Your login code", sentEmail.Subject);
        }

        [Fact]
        public async Task ResendPasswordlessLogin_WithNonExistentEmail_ReturnsOkButNoEmail()
        {
            // Arrange
            var request = new ResendPasswordlessLoginRequest(Email: "nonexistent@example.com");
            _emailSender.SentEmails.Clear();

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ResendPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(_emailSender.SentEmails);
        }

        [Fact]
        public async Task ResendPasswordlessLogin_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new ResendPasswordlessLoginRequest(Email: "invalid-email");
            _emailSender.SentEmails.Clear();

            // Act
            var response = await _client.PostAsJsonAsync(
                ApiEndpoints.IdentityEndpoints.ResendPasswordlessLogin,
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(_emailSender.SentEmails);
        }
    }
}