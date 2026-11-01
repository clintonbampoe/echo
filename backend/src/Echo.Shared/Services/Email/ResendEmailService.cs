using Echo.Shared.Models;
using Resend;
using ResendMessage = Resend.EmailMessage;

namespace Echo.Shared.Services.Email;

public class ResendEmailService(IResend resend) : IEmailService
{
    public async Task SendAsync(string to, IEmailContent content)
    {
        var message = new ResendMessage
        {
            From = "onboarding@resend.dev",
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
