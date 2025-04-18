using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using FluentValidation.TestHelper;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class PasswordlessLoginRequestValidatorTests
{
    private readonly PasswordlessLoginRequestValidator _validator;

    public PasswordlessLoginRequestValidatorTests()
    {
        _validator = new PasswordlessLoginRequestValidator();
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordlessLoginRequest(string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordlessLoginRequest("invalid-email-format");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is not valid.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("firstname.lastname@example.com")]
    [InlineData("email@subdomain.example.com")]
    [InlineData("firstname+lastname@example.com")]
    [InlineData("email@123.123.123.123")]
    [InlineData("1234567890@example.com")]
    [InlineData("email@example-one.com")]
    [InlineData("_______@example.com")]
    [InlineData("email@example.name")]
    [InlineData("email@example.museum")]
    [InlineData("email@example.co.jp")]
    [InlineData("firstname-lastname@example.com")]
    public void Validate_WhenEmailIsValid_ShouldNotHaveValidationError(string validEmail)
    {
        // Arrange
        var request = new PasswordlessLoginRequest(validEmail);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new PasswordlessLoginRequest("valid@example.com");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}