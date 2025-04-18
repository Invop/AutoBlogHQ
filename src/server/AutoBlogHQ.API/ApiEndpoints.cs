namespace AutoBlogHQ.API;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class IdentityEndpoints
    {
        private const string IdentityBase = $"/{ApiBase}/identity";
        public const string Register = $"{IdentityBase}/register";
        public const string ResendConfirmationEmail = $"{IdentityBase}/resendConfirmationEmail";
        public const string ConfirmEmail = $"{IdentityBase}/confirmEmail";
        public const string Login = $"{IdentityBase}/login";
        public const string ForgotPassword = $"{IdentityBase}/password/forgot";
        public const string ResetPassword = $"{IdentityBase}/password/reset";
        public const string PasswordlessLogin = $"{IdentityBase}/passwordless/login";
        public const string VerifyPasswordlessLogin = $"{IdentityBase}/passwordless/verify";
        public const string ResendPasswordlessLogin = $"{IdentityBase}/passwordless/resend";
        public const string Logout = $"{IdentityBase}/logout";
        public const string ChangePassword = $"{IdentityBase}/me/password/change";

        public const string TestProtected = $"{IdentityBase}/testProtected";
    }
}