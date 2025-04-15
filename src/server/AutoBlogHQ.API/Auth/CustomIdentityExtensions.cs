using Microsoft.AspNetCore.Identity;

namespace AutoBlogHQ.API.Auth;

public static class CustomIdentityExtensions
{
    public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        var provider = typeof(PasswordlessLoginTokenProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginProvider", provider);
    }
}