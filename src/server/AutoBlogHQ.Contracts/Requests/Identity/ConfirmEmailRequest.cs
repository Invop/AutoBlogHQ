namespace AutoBlogHQ.Contracts.Requests.Identity;

public record ConfirmEmailRequest(string UserId, string Code);