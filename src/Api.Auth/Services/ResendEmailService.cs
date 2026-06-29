using Api.Auth.Services.Interfaces;
using Resend;

namespace Api.Auth.Services;

public class ResendEmailService(IResend resend) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new EmailMessage
        {
            From = "noreply@theechoapp.net",
            Subject = subject,
            HtmlBody = htmlBody
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
