using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.IdentityAdditional.Passwordless;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class VerifyPasswordlessLoginEndpoint
{
    public static IEndpointRouteBuilder MapVerifyPasswordlessLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.AdditionalIdentityEndpoints.VerifyPasswordlessLogin,
                async Task<Results<Ok<string>, BadRequest<string>, UnauthorizedHttpResult>> (
                    [FromBody] VerifyPasswordLessCodeRequest request,
                    [FromQuery] bool? rememberMe,
                    [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                    var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();

                    if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
                        return TypedResults.BadRequest("Email and verification code are required");

                    var user = await userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                        return TypedResults.Unauthorized();

                    var isValid = await userManager.VerifyUserTokenAsync(
                        user,
                        "PasswordlessLoginTotpProvider",
                        "passwordless-auth",
                        request.Code);

                    if (!isValid)
                        return TypedResults.Unauthorized();

                    await userManager.UpdateSecurityStampAsync(user);
                    signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

                    var isPersistent = rememberMe == true;
                    await signInManager.SignInAsync(user, isPersistent);

                    return TypedResults.Ok("Signed in successfully");
                })
            .WithOpenApi();

        return app;
    }
}