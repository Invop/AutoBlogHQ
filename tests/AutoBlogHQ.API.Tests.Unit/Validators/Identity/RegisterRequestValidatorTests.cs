using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class RegisterRequestValidatorTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _validator = new RegisterRequestValidator(_userManagerMock.Object);
    }

    [Fact]
    public async Task Validate_WhenUserNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RegisterRequest(
            string.Empty,
            "test@example.com",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("UserName is required.");
    }

    [Theory]
    [InlineData("abc")] // Too short
    [InlineData("abcdefghijklmnopqrstuvwxyz")] // Too long
    public async Task Validate_WhenUserNameLengthInvalid_ShouldHaveValidationError(string username)
    {
        // Arrange
        var request = new RegisterRequest(
            username,
            "test@example.com",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        if (username.Length < 5)
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                .WithErrorMessage("UserName must be at least 5 characters long.");
        else
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                .WithErrorMessage("UserName must not exceed 20 characters.");
    }

    [Fact]
    public async Task Validate_WhenUserNameAlreadyTaken_ShouldHaveValidationError()
    {
        // Arrange
        var existingUser = new ApplicationUser { UserName = "existingUser" };
        _userManagerMock.Setup(x => x.FindByNameAsync("existingUser"))
            .ReturnsAsync(existingUser);

        var request = new RegisterRequest(
            "existingUser",
            "test@example.com",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("The specified username is already taken.");
    }

    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RegisterRequest(
            "validUser",
            string.Empty,
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RegisterRequest(
            "validUser",
            "invalid-email",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is not valid.");
    }

    [Fact]
    public async Task Validate_WhenEmailAlreadyInUse_ShouldHaveValidationError()
    {
        // Arrange
        var existingUser = new ApplicationUser { Email = "existing@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);

        var request = new RegisterRequest(
            "validUser",
            "existing@example.com",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is already in use.");
    }

    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RegisterRequest(
            "validUser",
            "test@example.com",
            string.Empty);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("nouppercase1!")] // No uppercase
    [InlineData("NoNumberHere!")] // No number
    [InlineData("MissingSpecialChar1")] // No special char
    public async Task Validate_WhenPasswordInvalid_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new RegisterRequest(
            "validUser",
            "test@example.com",
            password);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        if (password.Length < 6)
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must be at least 6 characters long.");
        else if (!password.Any(char.IsUpper))
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must contain at least one uppercase letter.");
        else if (!password.Any(char.IsDigit))
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must contain at least one number.");
        else
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must contain at least one special character.");
    }

    [Fact]
    public async Task Validate_WhenAllFieldsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByNameAsync("validUser"))
            .ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((ApplicationUser)null);

        var request = new RegisterRequest(
            "validUser",
            "test@example.com",
            "Valid1Password!");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}