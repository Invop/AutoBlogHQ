using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(r => r.ResetCode)
            .NotEmpty().WithMessage("Reset code is required.");
    }
}