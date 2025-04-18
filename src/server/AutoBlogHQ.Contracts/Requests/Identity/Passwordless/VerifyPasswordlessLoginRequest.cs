namespace AutoBlogHQ.Contracts.Requests.Identity.Passwordless;

public record VerifyPasswordlessLoginRequest(string Email, string Code, bool RememberMe = false);