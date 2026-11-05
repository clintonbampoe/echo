using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.Passwords;

public class PasswordResetEmailBody(string firstName, string resetLink) : IEmailContent
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
                    $"{EmailLayout.Greeting(firstName)} we received a request to reset the password for your Echo account. Select the button below to choose a new one.",
                ],
                Button = new EmailButton("Choose a new password", resetLink),
                Note = "This link works for 1 hour.",
                ShowLinkFallback = true,
                Reason = "If you did not request this, ignore this email. Your password will not change.",
            }
        );
}
