using Echo.Application.Models;
using Echo.Application.Services;
using Resend;

namespace Echo.Auth.Services.Email;

public class ResendEmailService(IResend resend) : IEmailService
{
    public async Task SendAsync(string to, IEmailContent content)
    {
        var message = new EmailMessage
        {
            From = "noreply@theechoapp.net",
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
