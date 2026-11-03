using Echo.Shared.Models;
using Echo.Shared.Options.Mail;
using Microsoft.Extensions.Options;
using Resend;
using ResendMessage = Resend.EmailMessage;

namespace Echo.Shared.Services.Email;

public class ResendEmailService(IResend resend, IOptions<MailClientOptions> options) : IEmailService
{
    public async Task SendAsync(string to, IEmailContent content)
    {
        var message = new ResendMessage
        {
            From = options.Value.Address,
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
