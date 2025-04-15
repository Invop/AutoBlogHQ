using System.Security.Claims;
using AutoBlogHQ.API.Auth;
using AutoBlogHQ.Application.Database;
using AutoBlogHQ.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBlogHQ.API;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    public static async Task CreateDefaultAuthorAsync(this IApplicationBuilder app, IConfiguration configuration)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var authorEmail = configuration["DefaultAuthor:Email"]
                          ?? throw new InvalidOperationException("Default author email is not configured");
        var authorPassword = configuration["DefaultAuthor:Password"]
                             ?? throw new InvalidOperationException("Default author password is not configured");

        // Check if the author already exists
        var existingUser = await userManager.FindByEmailAsync(authorEmail);
        if (existingUser != null)
            return;
        var user = new ApplicationUser
        {
            UserName = authorEmail,
            Email = authorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, authorPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create default author: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        await userManager.AddClaimAsync(user, new Claim(AuthConstants.AdminUserClaimName, "true"));
    }
}