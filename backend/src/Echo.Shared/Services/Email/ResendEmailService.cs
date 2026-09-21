using Echo.Shared.Models;
using Resend;

namespace Echo.Shared.Services.Email;

public class ResendEmailService(IResend resend) : IEmailService
{
    public async Task SendAsync(string to, IEmailContent content)
    {
        var message = new EmailMessage
        {
            From = "onboarding@resend.dev",
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
