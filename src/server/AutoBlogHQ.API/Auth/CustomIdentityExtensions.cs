using Microsoft.AspNetCore.Identity;

namespace AutoBlogHQ.API.Auth;

public static class CustomIdentityExtensions
{
    public static IdentityBuilder AddPasswordlessLoginTotpTokenProvider(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        var totpProvider = typeof(PasswordlessLoginTotpTokenProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginTotpProvider", totpProvider);
    }
}