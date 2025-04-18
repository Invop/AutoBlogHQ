using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation.TestHelper;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator;

    public ResetPasswordRequestValidatorTests()
    {
        _validator = new ResetPasswordRequestValidator();
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            string.Empty,
            NewPassword: "Valid1Password!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "invalid-email",
            NewPassword: "Valid1Password!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is not valid.");
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: string.Empty,
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password is required.");
    }

    [Fact]
    public void Validate_WhenPasswordTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "A1!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must be at least 6 characters long.");
    }

    [Fact]
    public void Validate_WhenPasswordNoUpperCase_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "password1!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Validate_WhenPasswordNoNumber_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "Password!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one number.");
    }

    [Fact]
    public void Validate_WhenPasswordNoSpecialChar_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "Password1",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one special character.");
    }

    [Fact]
    public void Validate_WhenResetCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "Valid1Password!",
            ResetCode: string.Empty
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResetCode)
            .WithErrorMessage("Reset code is required.");
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            "valid@example.com",
            NewPassword: "Valid1Password!",
            ResetCode: "validResetCode"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}