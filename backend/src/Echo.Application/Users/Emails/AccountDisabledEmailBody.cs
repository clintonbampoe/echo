using System.Globalization;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Application.Users.Emails;

/// <summary>Sent when an admin disables a person's account. There is nothing for them to click.</summary>
public class AccountDisabledEmailBody(
    string firstName,
    string congregationName,
    string emailAddress,
    DateTime disabledAtUtc
) : IEmailContent
{
    public string Subject => $"Your Echo account at {congregationName} is disabled";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Account",
                Heading = "Your account is disabled",
                Preheader = "You cannot sign in until an admin enables it again.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} an admin at {congregationName} disabled your Echo account. You are signed out on all devices and cannot sign in until the account is enabled again. Nothing was deleted.",
                ],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Account", emailAddress),
                    new EmailRow("Disabled", When),
                ],
                Callout = new EmailCallout(
                    "Think this is a mistake?",
                    $"Ask an admin at {congregationName} to enable your account."
                ),
                Reason = "We send this email when an admin disables your account.",
            }
        );

    private string When =>
        disabledAtUtc.ToString("d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture) + " UTC";
}
