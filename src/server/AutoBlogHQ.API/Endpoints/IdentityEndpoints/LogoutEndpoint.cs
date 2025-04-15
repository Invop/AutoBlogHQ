using AutoBlogHQ.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogout(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.AdditionalIdentityEndpoints.Logout, async ([FromServices] IServiceProvider sp,
                [FromBody] object? empty) =>
            {
                if (empty == null) return Results.Unauthorized();
                var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
                await signInManager.SignOutAsync();
                return Results.Ok();
            })
            .WithOpenApi()
            .RequireAuthorization();
        return app;
    }
}