using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.IdentityAdditional.Passwordless;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class PasswordlessLoginEndpoint
{
    public static IEndpointRouteBuilder MapPasswordlessLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.AdditionalIdentityEndpoints.PasswordlessLogin,
                async Task<Results<Ok<string>, ProblemHttpResult>> (
                    [FromBody] SendPasswordlessCodeRequest request,
                    [FromServices] IServiceProvider sp) =>
                {
                    // Resolve services from IServiceProvider
                    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                    var emailSender = sp.GetRequiredService<IEmailSender>();

                    // Validate email
                    if (string.IsNullOrEmpty(request.Email))
                        return TypedResults.Problem("Email is required", null, 400);

                    // Find user by email
                    var user = await userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                        // Don't reveal whether the user exists for security reasons
                        return TypedResults.Ok("If the email exists, a code will be sent.");

                    // Generate passwordless login token
                    var code = await userManager.GenerateUserTokenAsync(
                        user,
                        "PasswordlessLoginProvider",
                        "passwordless-auth");

                    // Send email with the code
                    var emailSubject = "Your login code";
                    var emailMessage = $"Your verification code is: {code}";
                    await emailSender.SendEmailAsync(request.Email, emailSubject, emailMessage);

                    return TypedResults.Ok($"If the email exists, a code will be sent.{emailMessage}");
                })
            .WithOpenApi();

        return app;
    }
}