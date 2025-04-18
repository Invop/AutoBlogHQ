using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class VerifyPasswordlessLoginRequestValidator : AbstractValidator<VerifyPasswordlessLoginRequest>
{
    public VerifyPasswordlessLoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("Verification code is required.");

        RuleFor(r => r.RememberMe)
            .NotNull().WithMessage("RememberMe is required.");
    }
}