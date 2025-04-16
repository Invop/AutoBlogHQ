using AutoBlogHQ.Application.Models;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        //TODO : NOT FINAL
        //TODO : CUSTOM Mail
        app.MapGroup(ApiEndpoints.AdditionalIdentityEndpoints.IdentityBase).MapIdentityApi<ApplicationUser>();
        app.MapPasswordlessLogin();
        app.MapResendPasswordlessLogin();
        app.MapVerifyPasswordlessLogin();
        app.MapLogout();
        return app;
    }
}