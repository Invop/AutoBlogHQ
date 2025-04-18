namespace AutoBlogHQ.API.Mail;

/// <summary>
///     Interface for sending different types of emails in the application
/// </summary>
public interface IEmailSender
{
    /// <summary>
    ///     Sends a general email with the specified subject and message
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="subject">The email subject</param>
    /// <param name="message">The email message content (HTML)</param>
    Task SendEmailAsync(string email, string subject, string message);

    /// <summary>
    ///     Sends an account confirmation email with a verification link
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="confirmationLink">The confirmation link URL</param>
    Task SendConfirmationEmailAsync(string email, string confirmationLink);

    /// <summary>
    ///     Sends a password reset email with a reset link
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="resetLink">The password reset link URL</param>
    Task SendPasswordResetLinkAsync(string email, string resetLink);

    /// <summary>
    ///     Sends a password reset email with a reset code
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="resetCode">The password reset code</param>
    Task SendPasswordResetCodeAsync(string email, string resetCode);

    /// <summary>
    ///     Sends a passwordless login email with an authentication code
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="loginCode">The passwordless login code</param>
    Task SendPasswordlessLoginCodeAsync(string email, string loginCode);
}