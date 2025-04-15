using AutoBlogHQ.API.Endpoints.IdentityEndpoints;

namespace AutoBlogHQ.API.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapIdentityEndpoints();
        return app;
    }
}