using System.Net;
using System.Net.Http.Json;
using AutoBlogHQ.Contracts.Requests.Identity;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class LoginIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly AutoBlogApiFactory _factory;

    public LoginIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private LoginRequest CreateLoginRequest(
        string userName = "",
        string password = "",
        bool rememberMe = true)
    {
        var testUser = _factory.TestUsers.First();

        return new LoginRequest(
            string.IsNullOrEmpty(userName) ? testUser.UserName : userName,
            string.IsNullOrEmpty(password) ? testUser.Password : password,
            rememberMe
        );
    }


    private LoginRequest CreateInvalidLoginRequest(
        string userName = "",
        string password = "",
        bool? rememberMe = null)
    {
        return new LoginRequest(
            userName,
            password,
            rememberMe ?? false
        );
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200()
    {
        // Arrange
        var request = CreateLoginRequest(rememberMe: true); // Explicit true

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.Login,
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("", "password", true)] // Empty username
    [InlineData("username", "", false)] // Empty password, rememberMe false
    public async Task Login_WithInvalidRequest_Returns400(string userName, string password, bool rememberMe)
    {
        // Arrange
        var request = CreateInvalidLoginRequest(
            userName,
            password,
            rememberMe
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.Login,
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithRememberMeFalse_Returns200()
    {
        // Arrange
        var request = CreateLoginRequest(rememberMe: false); // Explicit false

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.Login,
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        // Arrange
        var request = CreateLoginRequest(password: "wrong_password");

        // Act
        var response = await _client.PostAsJsonAsync(
            ApiEndpoints.IdentityEndpoints.Login,
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestProtected_WithValidAuthAndRememberMeFalse_Returns200()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var authClient = await _factory.GetAuthenticatedClientAsync(
            testUser.UserName,
            testUser.Password
        );

        // Act
        var response = await authClient.GetAsync(
            ApiEndpoints.IdentityEndpoints.TestProtected
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}