namespace AutoBlogHQ.Contracts.Requests.Identity;

public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);