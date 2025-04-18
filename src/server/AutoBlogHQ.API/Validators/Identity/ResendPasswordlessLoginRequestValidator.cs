using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class ResendPasswordlessLoginRequestValidator : AbstractValidator<ResendPasswordlessLoginRequest>
{
    public ResendPasswordlessLoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
    }
}