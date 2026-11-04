using System.Globalization;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.Passwords;

public class PasswordChangedEmailBody(
    string firstName,
    string emailAddress,
    DateTime changedAtUtc,
    string forgotPasswordLink,
    string siteUrl
) : IEmailContent
{
    public string Subject => "Your Echo password was changed";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Security",
                Heading = "Your password was changed",
                Preheader = "If this was not you, reset it now.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} the password for your Echo account was changed. If this was you, there is nothing more to do.",
                ],
                Rows =
                [
                    new EmailRow("When", When),
                    new EmailRow("Account", emailAddress),
                ],
                Callout = new EmailCallout(
                    "Was this not you?",
                    "Reset your password straight away."
                ),
                Button = new EmailButton("Reset password", forgotPasswordLink),
                Reason = "We send this every time a password changes, so you always know.",
                SiteUrl = siteUrl,
            }
        );

    private string When =>
        changedAtUtc.ToString("d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture) + " UTC";
}
