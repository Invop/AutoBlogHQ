using Microsoft.AspNetCore.Identity;

namespace AutoBlogHQ.API.Auth;

public class PasswordlessLoginTotpTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : class
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var email = await manager.GetEmailAsync(user);
        return "PasswordlessLogin:" + purpose + ":" + email;
    }
}