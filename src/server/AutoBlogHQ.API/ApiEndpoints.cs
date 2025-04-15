namespace AutoBlogHQ.API;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class AdditionalIdentityEndpoints
    {
        public const string IdentityBase = $"/{ApiBase}/identity";
        public const string PasswordlessLogin = $"{IdentityBase}/passwordless/login";
        public const string VerifyPasswordlessLogin = $"{IdentityBase}/passwordless/verify";
        public const string Logout = $"{IdentityBase}/logout";
    }
}