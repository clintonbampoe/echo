using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.EmailVerifications;

public class EmailVerificationMessageBody(string firstName, string verificationLink)
    : IEmailContent
{
    public string Subject => "Verify your Echo account";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Account",
                Heading = "Verify your account",
                Preheader = "Verify your account to finish setting it up.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} welcome to Echo. Verify your account to finish setting it up.",
                ],
                Button = new EmailButton("Verify account", verificationLink),
                Note = "This link works for 24 hours.",
                ShowLinkFallback = true,
                Reason = "You are getting this because an Echo account was created with this address. If that was not you, ignore this message and nothing will happen.",
            }
        );
}
