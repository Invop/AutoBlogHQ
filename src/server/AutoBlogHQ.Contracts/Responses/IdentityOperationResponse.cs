namespace AutoBlogHQ.Contracts.Responses;

public record IdentityOperationResponse(bool Succeeded, IEnumerable<string>? Errors);