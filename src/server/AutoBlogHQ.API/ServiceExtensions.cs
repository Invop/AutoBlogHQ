using AutoBlogHQ.API.Auth;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.Application.Database;
using AutoBlogHQ.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AutoBlogHQ.API;

public static class ServiceExtensions
{
    // Method to configure Authentication and Authorization.
    public static IServiceCollection AddCustomAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredLength = 6;
        });

        services.Configure<PasswordlessLoginTokenProviderOptions>(x => x.TokenLifespan = TimeSpan.FromMinutes(5));
        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme,
                options =>
                {
                    options.LoginPath = ApiEndpoints.IdentityEndpoints.Login;
                    options.LogoutPath = ApiEndpoints.IdentityEndpoints.Logout;
                });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.AdminUserPolicyName, policy =>
                policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
        });
        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddPasswordlessLoginTotpTokenProvider();
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>();
        services.AddValidatorsFromAssemblyContaining<Application.IApplicationMarker>();
        return services;
    }

    public static IServiceCollection AddMailer(this IServiceCollection services)
    {
        services.AddTransient<IEmailSender, DefaultApplicationEmailSender>();
        return services;
    }

    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }
}