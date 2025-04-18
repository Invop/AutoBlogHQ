using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Contracts.Requests.Identity;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class RegisterIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly MockEmailSender? _emailSender;
    private readonly AutoBlogApiFactory _factory;

    public RegisterIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _emailSender = factory.Services.GetRequiredService<IEmailSender>() as MockEmailSender;
    }


    private RegisterRequest GenerateValidRegisterRequest()
    {
        var faker = new Faker();

        return new RegisterRequest(
            faker.Internet.UserName(),
            faker.Internet.Email(),
            "Valid1Password!"
        );
    }

    private RegisterRequest WithModifiedValues(
        RegisterRequest original,
        string userName = "",
        string email = "",
        string password = "")
    {
        return new
            RegisterRequest(string.IsNullOrEmpty(userName) ? original.UserName : userName,
                string.IsNullOrEmpty(email) ? original.Email : email,
                string.IsNullOrEmpty(password) ? original.Password : password);
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var request = GenerateValidRegisterRequest();
        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.Register, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("", "", "weak")]
    [InlineData("", "not-an-email", "")]
    [InlineData("invalid user", "", "")]
    public async Task Register_WithInvalidData_Returns400(string password, string email, string userName)
    {
        var validRequest = GenerateValidRegisterRequest();
        var invalidRequest = WithModifiedValues(validRequest, userName, email, password);

        var response = await _client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.Register, invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}