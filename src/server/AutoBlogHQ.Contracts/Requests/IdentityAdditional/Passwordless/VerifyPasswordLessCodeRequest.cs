namespace AutoBlogHQ.Contracts.Requests.IdentityAdditional.Passwordless;

public record VerifyPasswordLessCodeRequest(string Email, string Code);