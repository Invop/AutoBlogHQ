namespace AutoBlogHQ.Contracts.Requests.Identity;

public record LoginRequest(string UserName, string Password, bool RememberMe);