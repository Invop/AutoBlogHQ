using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation.TestHelper;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_WhenUserNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new LoginRequest(
            string.Empty,
            "validPassword123",
            false
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName is required.");
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new LoginRequest(
            "validUsername",
            string.Empty,
            false
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Validate_WhenBothFieldsEmpty_ShouldHaveBothValidationErrors()
    {
        // Arrange
        var request = new LoginRequest(
            string.Empty,
            string.Empty,
            false
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName is required.");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("username", "password")]
    [InlineData("testUser", "testPass123")]
    [InlineData("admin", "Admin@123")]
    [InlineData("user.name", "P@ssw0rd")]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationErrors(string username, string password)
    {
        // Arrange
        var request = new LoginRequest(
            username,
            password,
            false
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}