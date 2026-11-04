using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.Passwords;

public class PasswordResetEmailBody(string firstName, string resetLink, string siteUrl)
    : IEmailContent
{
    public string Subject => "Reset your Echo password";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Password",
                Heading = "Reset your password",
                Preheader = "Choose a new password. The link works for 1 hour.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} we got a request to reset the password for your Echo account. Choose a new one below.",
                ],
                Button = new EmailButton("Choose a new password", resetLink),
                Note = "This link works for 1 hour.",
                ShowLinkFallback = true,
                Reason = "If you did not ask for this, ignore this message. Your password stays the same.",
                SiteUrl = siteUrl,
            }
        );
}
