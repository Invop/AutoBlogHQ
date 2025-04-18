using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using FluentValidation.TestHelper;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class VerifyPasswordlessLoginRequestValidatorTests
{
    private readonly VerifyPasswordlessLoginRequestValidator _validator;

    public VerifyPasswordlessLoginRequestValidatorTests()
    {
        _validator = new VerifyPasswordlessLoginRequestValidator();
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new VerifyPasswordlessLoginRequest(
            string.Empty,
            "123456",
            true
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
        var request = new VerifyPasswordlessLoginRequest(
            "invalid-email",
            "123456",
            true
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is not valid.");
    }

    [Theory]
    [InlineData("valid@example.com")]
    [InlineData("user.name@domain.co")]
    [InlineData("firstname.lastname@example.com")]
    public void Validate_WhenEmailIsValid_ShouldNotHaveValidationError(string validEmail)
    {
        // Arrange
        var request = new VerifyPasswordlessLoginRequest(
            validEmail,
            "123456",
            true
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new VerifyPasswordlessLoginRequest(
            "valid@example.com",
            string.Empty,
            true
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Verification code is required.");
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WhenRememberMeHasValue_ShouldNotHaveValidationError(bool rememberMe)
    {
        // Arrange
        var request = new VerifyPasswordlessLoginRequest(
            "valid@example.com",
            "123456",
            rememberMe
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RememberMe);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new VerifyPasswordlessLoginRequest(
            "valid@example.com",
            "123456",
            true
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}