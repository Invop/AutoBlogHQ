using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;

namespace AutoBlogHQ.API.Validators.Identity;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(r => r.Code)
            .NotEmpty()
            .Must(BeValidBase64UrlEncoding);
        RuleFor(r => r.UserId)
            .NotEmpty()
            .Must(BeValidGuid);
    }

    private bool BeValidBase64UrlEncoding(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        try
        {
            WebEncoders.Base64UrlDecode(code);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private bool BeValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}