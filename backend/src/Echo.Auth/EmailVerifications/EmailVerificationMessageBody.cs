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
                Preheader = "Verify your account to start using Echo.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} thanks for signing up for Echo. Select the button below to verify your account and start using it.",
                ],
                Button = new EmailButton("Verify account", verificationLink),
                Note = "This link works for 24 hours. After that, request a new one from Echo.",
                ShowLinkFallback = true,
                Reason = "You are getting this because an Echo account was created with this email address. If that was not you, ignore this email and nothing will happen.",
            }
        );
}
