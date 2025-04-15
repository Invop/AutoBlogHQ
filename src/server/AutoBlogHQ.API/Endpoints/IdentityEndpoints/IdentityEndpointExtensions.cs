using AutoBlogHQ.Application.Models;

namespace AutoBlogHQ.API.Endpoints.IdentityEndpoints;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        //TODO : NOT FINAL
        app.MapGroup(ApiEndpoints.AdditionalIdentityEndpoints.IdentityBase).MapIdentityApi<ApplicationUser>();
        app.MapPasswordlessLogin();
        app.MapVerifyPasswordlessLogin();
        app.MapLogout();
        return app;
    }
}