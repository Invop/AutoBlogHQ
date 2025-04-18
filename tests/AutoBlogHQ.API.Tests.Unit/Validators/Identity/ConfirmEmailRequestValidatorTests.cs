using AutoBlogHQ.API.Validators.Identity;
using AutoBlogHQ.Contracts.Requests.Identity;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.WebUtilities;

namespace AutoBlogHQ.API.Tests.Unit.Validators.Identity;

public class ConfirmEmailRequestValidatorTests
{
    private readonly ConfirmEmailRequestValidator _validator;

    public ConfirmEmailRequestValidatorTests()
    {
        _validator = new ConfirmEmailRequestValidator();
    }

    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ConfirmEmailRequest(
            Code: string.Empty,
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("'Code' must not be empty.");
    }

    [Fact]
    public void Validate_WhenCodeIsInvalidBase64Url_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ConfirmEmailRequest(
            Code: "invalid-base64-url!@#$",
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_WhenCodeIsValidBase64Url_ShouldNotHaveValidationError()
    {
        // Arrange
        var validCode = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        var request = new ConfirmEmailRequest(
            Code: validCode,
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var validCode = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        var request = new ConfirmEmailRequest(
            Code: validCode,
            UserId: string.Empty
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenUserIdIsInvalidGuid_ShouldHaveValidationError()
    {
        // Arrange
        var validCode = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        var request = new ConfirmEmailRequest(
            Code: validCode,
            UserId: "not-a-valid-guid"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenUserIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        var validCode = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        var request = new ConfirmEmailRequest(
            Code: validCode,
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var validCode = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray());
        var request = new ConfirmEmailRequest(
            Code: validCode,
            UserId: Guid.NewGuid().ToString()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}