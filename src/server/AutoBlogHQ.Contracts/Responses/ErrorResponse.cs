namespace AutoBlogHQ.Contracts.Responses;

public record ErrorResponse(string Message, IEnumerable<string>? Errors);