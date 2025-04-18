using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using AutoBlogHQ.API.Mail;
using AutoBlogHQ.API.Mapping;
using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;
using AutoBlogHQ.Contracts.Requests.Identity.Passwordless;
using AutoBlogHQ.Contracts.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AutoBlogHQ.API.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    // Validator fields
    private readonly IValidator<ChangePasswordRequest> _changePasswordRequestValidator;
    private readonly IValidator<ConfirmEmailRequest> _confirmEmailRequestValidator;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordRequestValidator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<IdentityController> _logger;
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<PasswordlessLoginRequest> _passwordlessLoginRequestValidator;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;
    private readonly IValidator<ResendConfirmationEmailRequest> _resendConfirmationEmailRequestValidator;
    private readonly IValidator<ResendPasswordlessLoginRequest> _resendPasswordlessLoginRequestValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordRequestValidator;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<VerifyPasswordlessLoginRequest> _verifyPasswordlessLoginRequestValidator;

    public IdentityController(
        ILogger<IdentityController> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        LinkGenerator linkGenerator,
        IValidator<RegisterRequest> registerRequestValidator,
        IValidator<LoginRequest> loginRequestValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordRequestValidator,
        IValidator<ResetPasswordRequest> resetPasswordRequestValidator,
        IValidator<PasswordlessLoginRequest> passwordlessLoginRequestValidator,
        IValidator<VerifyPasswordlessLoginRequest> verifyPasswordlessLoginRequestValidator,
        IValidator<ResendPasswordlessLoginRequest> resendPasswordlessLoginRequestValidator,
        IValidator<ChangePasswordRequest> changePasswordRequestValidator,
        IValidator<ResendConfirmationEmailRequest> resendConfirmationEmailRequestValidator,
        IValidator<ConfirmEmailRequest> confirmEmailRequestValidator)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _linkGenerator = linkGenerator;
        _registerRequestValidator = registerRequestValidator;
        _loginRequestValidator = loginRequestValidator;
        _forgotPasswordRequestValidator = forgotPasswordRequestValidator;
        _resetPasswordRequestValidator = resetPasswordRequestValidator;
        _passwordlessLoginRequestValidator = passwordlessLoginRequestValidator;
        _verifyPasswordlessLoginRequestValidator = verifyPasswordlessLoginRequestValidator;
        _resendPasswordlessLoginRequestValidator = resendPasswordlessLoginRequestValidator;
        _changePasswordRequestValidator = changePasswordRequestValidator;
        _resendConfirmationEmailRequestValidator = resendConfirmationEmailRequestValidator;
        _confirmEmailRequestValidator = confirmEmailRequestValidator;
    }

    [Authorize]
    [HttpGet(ApiEndpoints.IdentityEndpoints.TestProtected)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult TestProtected()
    {
        return Ok(new
        {
            Message = "This is a protected endpoint"
        });
    }


    #region Register Login Logout

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.Register)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken token)
    {
        await _registerRequestValidator.ValidateAndThrowAsync(request, token);

        var newUser = request.MapToApplicationUser();
        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(
                "User registration failed",
                result.Errors.Select(e => e.Description)
            ));

        _logger.LogInformation("New user {Email} registered successfully", request.Email);
        await SendConfirmationEmailAsync(newUser, HttpContext, request.Email);
        return StatusCode(StatusCodes.Status201Created);
    }

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.Login)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken token)
    {
        await _loginRequestValidator.ValidateAndThrowAsync(request, token);

        _logger.LogInformation("Login attempt for user {Email}", request.UserName);
        _signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

        var result = await _signInManager.PasswordSignInAsync(
            request.UserName,
            request.Password,
            request.RememberMe,
            true);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for user {Email}", request.UserName);
            return Unauthorized();
        }

        _logger.LogInformation("User {Email} logged in successfully", request.UserName);
        return Ok();
    }

    [Authorize]
    [HttpPost(ApiEndpoints.IdentityEndpoints.Logout)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out", userId);
        return Ok();
    }

    #endregion

    #region Confirmation Email

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.ResendConfirmationEmail)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationEmail(
        [FromBody] ResendConfirmationEmailRequest request,
        CancellationToken token)
    {
        await _resendConfirmationEmailRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            _logger.LogInformation("Confirmation email resend requested for non-existent email: {Email}",
                request.Email);
            return Accepted();
        }

        await SendConfirmationEmailAsync(user, HttpContext, request.Email);
        _logger.LogInformation("Confirmation email resent to {Email}", request.Email);

        return Accepted();
    }

    [AllowAnonymous]
    [HttpGet(ApiEndpoints.IdentityEndpoints.ConfirmEmail, Name = nameof(ConfirmEmail))]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request,
        CancellationToken token)
    {
        await _confirmEmailRequestValidator.ValidateAndThrowAsync(request, token);
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return NotFound(new ErrorResponse("User not found", null));

        string code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
        }
        catch (FormatException)
        {
            return Unauthorized(new ErrorResponse("Invalid token format", null));
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(
                "Email confirmation failed",
                result.Errors.Select(e => e.Description)
            ));
        await _userManager.UpdateSecurityStampAsync(user);

        _logger.LogInformation("Email confirmed for user {Id}", user.Id);
        return Ok(new SuccessResponse("Thank you for confirming your email!"));
    }

    #endregion

    #region Password Reset And Change

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.ForgotPassword)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken token)
    {
        await _forgotPasswordRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            await _emailSender.SendPasswordResetCodeAsync(request.Email, HtmlEncoder.Default.Encode(code));
            _logger.LogInformation("Password reset code sent to {Email}", request.Email);
        }
        else
        {
            // Don't reveal that the user does not exist or is not confirmed
            _logger.LogInformation("Password reset requested for non-existent or unconfirmed email: {Email}",
                request.Email);
        }

        // Return Accepted regardless of whether user exists to prevent user enumeration
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.ResetPassword)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken token)
    {
        await _resetPasswordRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            // Don't reveal that the user does not exist or is not confirmed
            return BadRequest(new ErrorResponse("Invalid token", null));

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));
            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            _logger.LogWarning("Invalid reset code format for user {Email}", request.Email);
            return BadRequest(new ErrorResponse("Invalid token format", null));
        }

        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(
                "Password reset failed",
                result.Errors.Select(e => e.Description)
            ));

        _logger.LogInformation("Password reset successfully for user {Email}", request.Email);
        return Ok(new SuccessResponse("Password has been reset successfully"));
    }

    [Authorize]
    [HttpPut(ApiEndpoints.IdentityEndpoints.ChangePassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken token)
    {
        await _changePasswordRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(new ErrorResponse("New password and confirmation do not match", null));

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(
                "Change password failed",
                result.Errors.Select(e => e.Description)
            ));

        _logger.LogInformation("Password changed successfully for user {Id}", user.Id);
        return NoContent();
    }

    #endregion

    #region Passwordless

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.PasswordlessLogin)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PasswordlessLogin(
        [FromBody] PasswordlessLoginRequest request,
        CancellationToken token)
    {
        await _passwordlessLoginRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            _logger.LogInformation("Passwordless login requested for non-existent email: {Email}", request.Email);
            return Ok();
        }

        await SendPasswordlessLoginCodeAsync(user);
        _logger.LogInformation("Passwordless login code sent to {Email}", request.Email);

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.ResendPasswordlessLogin)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendPasswordlessLogin(
        [FromBody] ResendPasswordlessLoginRequest request,
        CancellationToken token)
    {
        await _resendPasswordlessLoginRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            _logger.LogInformation("Passwordless login code resend requested for non-existent email: {Email}",
                request.Email);
            return Ok();
        }

        await SendPasswordlessLoginCodeAsync(user);
        _logger.LogInformation("Passwordless login code resent to {Email}", request.Email);

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost(ApiEndpoints.IdentityEndpoints.VerifyPasswordlessLogin)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyPasswordlessLogin(
        [FromBody] VerifyPasswordlessLoginRequest request,
        CancellationToken token)
    {
        await _verifyPasswordlessLoginRequestValidator.ValidateAndThrowAsync(request, token);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Passwordless login verification attempted for non-existent user: {Email}",
                request.Email);
            return Unauthorized();
        }

        var isValid = await _userManager.VerifyUserTokenAsync(
            user,
            "PasswordlessLoginTotpProvider",
            "passwordless-auth",
            request.Code);

        if (!isValid)
        {
            _logger.LogWarning("Invalid passwordless login code for user {Email}", request.Email);
            return Unauthorized();
        }

        // Update security stamp to invalidate any previous sessions
        await _userManager.UpdateSecurityStampAsync(user);

        // Sign in the user
        _signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;
        await _signInManager.SignInAsync(user, request.RememberMe, "Passwordless");

        _logger.LogInformation("User {Email} logged in via passwordless authentication", request.Email);
        return Ok();
    }

    #endregion

    #region Helpers

    private async Task SendConfirmationEmailAsync(ApplicationUser user, HttpContext context, string email)
    {
        // Skip sending if email is already confirmed
        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Email already confirmed for {Email}, skipping confirmation email", email);
            return;
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var userId = await _userManager.GetUserIdAsync(user);

        var routeValues = new RouteValueDictionary
        {
            ["UserId"] = userId,
            ["Code"] = code
        };

        var confirmEmailUrl = _linkGenerator.GetUriByName(context, nameof(ConfirmEmail), routeValues)
                              ?? throw new NotSupportedException(
                                  $"Could not find endpoint named '{nameof(ConfirmEmail)}'.");
        await _emailSender.SendConfirmationEmailAsync(email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    private async Task SendPasswordlessLoginCodeAsync(ApplicationUser user)
    {
        await _userManager.UpdateSecurityStampAsync(user);
        var passwordlessToken = await _userManager.GenerateUserTokenAsync(
            user,
            "PasswordlessLoginTotpProvider",
            "passwordless-auth");

        if (user.Email != null)
            await _emailSender.SendPasswordlessLoginCodeAsync(user.Email, passwordlessToken);
    }

    #endregion
}