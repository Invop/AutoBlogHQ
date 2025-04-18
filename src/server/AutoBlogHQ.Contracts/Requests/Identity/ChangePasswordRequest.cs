namespace AutoBlogHQ.Contracts.Requests.Identity;

public record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmNewPassword);