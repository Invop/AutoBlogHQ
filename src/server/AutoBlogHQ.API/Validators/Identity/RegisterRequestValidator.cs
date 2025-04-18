using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AutoBlogHQ.API.Validators.Identity;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterRequestValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        RuleFor(r => r.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .MinimumLength(5).WithMessage("UserName must be at least 5 characters long.")
            .MaximumLength(20).WithMessage("UserName must not exceed 20 characters.")
            .MustAsync(ValidateUserNameUnique).WithMessage("The specified username is already taken.");


        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MustAsync(ValidateEmailUnique).WithMessage("Email is already in use.");


        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }

    private async Task<bool> ValidateEmailUnique(RegisterRequest request, string email,
        CancellationToken cancellationToken)
    {
        var owner = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        return owner == null;
    }

    private async Task<bool> ValidateUserNameUnique(string userName, CancellationToken cancellationToken)
    {
        var owner = await _userManager.FindByNameAsync(userName).ConfigureAwait(false);
        return owner == null;
    }
}