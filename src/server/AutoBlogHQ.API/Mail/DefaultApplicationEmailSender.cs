namespace AutoBlogHQ.API.Mail;

public class DefaultApplicationEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        throw new NotImplementedException();
    }

    public Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordlessLoginCodeAsync(string email, string loginCode)
    {
        throw new NotImplementedException();
    }
}