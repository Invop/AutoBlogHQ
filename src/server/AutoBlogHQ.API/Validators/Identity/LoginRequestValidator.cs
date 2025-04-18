using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.UserName)
            .NotEmpty().WithMessage("UserName is required.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(r => r.RememberMe)
            .NotNull().WithMessage("RememberMe is required.");
    }
}