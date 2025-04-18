using System.Net.Http.Json;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Application.Database;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AutoBlogHQ.API.Tests.Integration;

public class AutoBlogApiFactory : WebApplicationFactory<IApplicationMarker>, IAsyncLifetime
{
    private readonly string _dbName = $"autoblogHQTests_{Guid.NewGuid()}";
    public List<TestUser> TestUsers { get; private set; } = new();

    public async Task InitializeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            await InitializeTestUsersAsync(scope.ServiceProvider);
        }
    }

    public new async Task DisposeAsync()
    {
        // Clean up database after tests complete
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }

    private async Task InitializeTestUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        TestUsers = new List<TestUser>
        {
            new()
            {
                Email = "admin@example.com",
                Password = "Password123!",
                UserName = "admin@example.com"
            },
            new()
            {
                Email = "user1@example.com",
                Password = "Password123!",
                UserName = "user1@example.com"
            }
        };

        foreach (var testUser in TestUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(testUser.Email);
            if (existingUser != null)
            {
                // User already exists, store its ID
                testUser.Id = existingUser.Id;
                continue;
            }

            var user = new ApplicationUser
            {
                Email = testUser.Email,
                UserName = testUser.UserName,
                EmailConfirmed = true // Set email as confirmed for testing purposes
            };

            var result = await userManager.CreateAsync(user, testUser.Password);

            if (result.Succeeded)
            {
                testUser.Id = user.Id;
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception($"Failed to create user {testUser.Email}: {errors}");
            }
        }

        foreach (var testUser in TestUsers)
        {
            var user = await userManager.FindByEmailAsync(testUser.Email);
            if (user == null) throw new Exception($"Failed to retrieve user {testUser.Email} after creation");

            var passwordValid = await userManager.CheckPasswordAsync(user, testUser.Password);
            if (!passwordValid) throw new Exception($"Password validation failed for user {testUser.Email}");
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IHostedService));

            services.RemoveAll(typeof(ApplicationDbContext));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    $"Server=localhost;Port=5432;Database={_dbName};User Id=changeme;Password=changeme;"));

            services.RemoveAll(typeof(IEmailSender));
            services.AddSingleton<IEmailSender, MockEmailSender>();
        });
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync(string userName, string password)
    {
        var client = CreateClient();
        await AuthenticateClientAsync(client, userName, password);
        return client;
    }

    private async Task AuthenticateClientAsync(HttpClient client, string userName, string password)
    {
        var loginRequest = new LoginRequest(userName, password, true);

        var response = await client.PostAsJsonAsync(ApiEndpoints.IdentityEndpoints.Login, loginRequest);

        response.EnsureSuccessStatusCode();
    }
}

public class TestUser
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string UserName { get; set; }
}