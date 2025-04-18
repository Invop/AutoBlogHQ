using AutoBlogHQ.API.Mail;

namespace AutoBlogHQ.API.Tests.Integration;

public class MockEmailSender : IEmailSender
{
    // Store sent emails for verification in tests
    public List<EmailMessage> SentEmails { get; } = new();

    public Task SendEmailAsync(string email, string subject, string message)
    {
        SentEmails.Add(new EmailMessage(email, subject, message));
        return Task.CompletedTask;
    }

    public Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        SentEmails.Add(new EmailMessage(email, "Confirm your email", confirmationLink));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(string email, string resetLink)
    {
        SentEmails.Add(new EmailMessage(email, "Reset your password", resetLink));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(string email, string resetCode)
    {
        SentEmails.Add(new EmailMessage(email, "Your password reset code", resetCode));
        return Task.CompletedTask;
    }

    public Task SendPasswordlessLoginCodeAsync(string email, string loginCode)
    {
        SentEmails.Add(new EmailMessage(email, "Your login code", loginCode));
        return Task.CompletedTask;
    }

    // Helper class to track email details
    public record EmailMessage(string Recipient, string Subject, string Content);
}