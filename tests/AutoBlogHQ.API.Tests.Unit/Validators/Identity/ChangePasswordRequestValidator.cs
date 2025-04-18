using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation.TestHelper;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator;

    public ChangePasswordRequestValidatorTests()
    {
        _validator = new ChangePasswordRequestValidator();
    }

    [Fact]
    public void Validate_WhenOldPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            string.Empty,
            "Valid1Password!",
            "Valid1Password!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OldPassword)
            .WithErrorMessage("Current password is required.");
    }

    [Fact]
    public void Validate_WhenNewPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            string.Empty,
            string.Empty
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password is required.");
    }

    [Fact]
    public void Validate_WhenNewPasswordTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "A1!",
            "A1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must be at least 6 characters long.");
    }

    [Fact]
    public void Validate_WhenNewPasswordNoUpperCase_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "password1!",
            "password1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Validate_WhenNewPasswordNoNumber_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "Password!",
            "Password!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must contain at least one number.");
    }

    [Fact]
    public void Validate_WhenNewPasswordNoSpecialChar_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "Password1",
            "Password1"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must contain at least one special character.");
    }

    [Fact]
    public void Validate_WhenNewPasswordSameAsOld_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "OldPassword1!",
            "OldPassword1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must not be the same as the current password.");
    }

    [Fact]
    public void Validate_WhenConfirmPasswordDoesNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "Valid1Password!",
            "Different1Password!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword)
            .WithErrorMessage("New passwords must match.");
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new ChangePasswordRequest(
            "OldPassword1!",
            "Valid1Password!",
            "Valid1Password!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}