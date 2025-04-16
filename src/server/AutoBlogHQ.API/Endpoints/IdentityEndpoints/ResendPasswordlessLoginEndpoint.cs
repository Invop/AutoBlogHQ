using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.IdentityAdditional.Passwordless;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class ResendPasswordlessLoginEndpoint
{
    public static IEndpointRouteBuilder MapResendPasswordlessLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.AdditionalIdentityEndpoints.ResendPasswordlessLogin,
                async Task<Results<Ok<string>, ProblemHttpResult>> (
                    [FromBody] ResendPasswordlessCodeRequest request,
                    [FromServices] IServiceProvider sp) =>
                {
                    if (string.IsNullOrEmpty(request.Email))
                        return TypedResults.Problem("Email is required", null, 400);

                    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                    var emailSender = sp.GetRequiredService<IEmailSender>();
                    var user = await userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                        return TypedResults.Ok("If the email exists, a code will be resent.");

                    var code = await userManager.GenerateUserTokenAsync(
                        user,
                        "PasswordlessLoginTotpProvider",
                        "passwordless-auth");
                    //TODO : CUSTOM Mail
                    var emailSubject = "Your login code (resend)";
                    var emailMessage = $"Your verification code is: {code}";
                    await emailSender.SendEmailAsync(request.Email, emailSubject, emailMessage);

                    return TypedResults.Ok("If the email exists, a code will be resent.");
                })
            .WithOpenApi();

        return app;
    }
}