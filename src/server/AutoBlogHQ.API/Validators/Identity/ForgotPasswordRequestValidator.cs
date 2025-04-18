using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
    }
}