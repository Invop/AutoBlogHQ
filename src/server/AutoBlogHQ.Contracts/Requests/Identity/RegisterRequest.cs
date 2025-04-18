namespace AutoBlogHQ.Contracts.Requests.Identity;

public record RegisterRequest(string UserName, string Email, string Password);