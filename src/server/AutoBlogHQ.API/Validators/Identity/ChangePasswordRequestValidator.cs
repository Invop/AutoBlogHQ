using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;

namespace AutoBlogHQ.API.Validators.Identity;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.OldPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character.")
            .NotEqual(r => r.OldPassword).WithMessage("New password must not be the same as the current password.");

        RuleFor(r => r.ConfirmNewPassword)
            .Equal(r => r.NewPassword).WithMessage("New passwords must match.");
    }
}