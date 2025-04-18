using System.Net;

namespace AutoBlogHQ.API.Tests.Integration.IdentityController;

public class LogoutIdentityControllerTests : IClassFixture<AutoBlogApiFactory>
{
    private readonly HttpClient _client;
    private readonly AutoBlogApiFactory _factory;

    public LogoutIdentityControllerTests(AutoBlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Logout_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsync(
            ApiEndpoints.IdentityEndpoints.Logout,
            null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var authClient = await _factory.GetAuthenticatedClientAsync(
            testUser.UserName,
            testUser.Password);

        // Act
        var response = await authClient.PostAsync(
            ApiEndpoints.IdentityEndpoints.Logout,
            null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_InvalidatesAuthentication()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var authClient = await _factory.GetAuthenticatedClientAsync(
            testUser.UserName,
            testUser.Password);

        // Verify pre-condition (authenticated)
        var protectedResponseBefore = await authClient.GetAsync(
            ApiEndpoints.IdentityEndpoints.TestProtected);
        Assert.Equal(HttpStatusCode.OK, protectedResponseBefore.StatusCode);

        // Act
        await authClient.PostAsync(
            ApiEndpoints.IdentityEndpoints.Logout,
            null);

        // Assert
        var protectedResponseAfter = await authClient.GetAsync(
            ApiEndpoints.IdentityEndpoints.TestProtected);
        Assert.Equal(HttpStatusCode.NotFound, protectedResponseAfter.StatusCode);
    }

    [Fact]
    public async Task Logout_AllowsRepeatedCalls()
    {
        // Arrange
        var testUser = _factory.TestUsers.First();
        var authClient = await _factory.GetAuthenticatedClientAsync(
            testUser.UserName,
            testUser.Password);

        // Act & Assert - First logout
        var firstResponse = await authClient.PostAsync(
            ApiEndpoints.IdentityEndpoints.Logout,
            null);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act & Assert - Second logout
        var secondResponse = await authClient.PostAsync(
            ApiEndpoints.IdentityEndpoints.Logout,
            null);
        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
    }
}