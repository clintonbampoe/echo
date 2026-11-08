using System.Globalization;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Application.Users.Emails;

/// <summary>Sent when an admin removes a person's account. There is nothing for them to click.</summary>
public class AccountRemovedEmailBody(
    string firstName,
    string congregationName,
    string emailAddress,
    DateTime removedAtUtc
) : IEmailContent
{
    public string Subject => $"Your Echo account at {congregationName} was removed";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Account",
                Heading = "Your account was removed",
                Preheader = $"You can no longer sign in to {congregationName} on Echo.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} an admin at {congregationName} removed your Echo account. You are signed out on all devices and can no longer sign in with it.",
                ],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Account", emailAddress),
                    new EmailRow("Removed", When),
                ],
                Callout = new EmailCallout(
                    "Want access again?",
                    $"Ask an admin at {congregationName} to invite you again."
                ),
                Reason = "We send this email when an admin removes your account.",
            }
        );

    private string When =>
        removedAtUtc.ToString("d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture) + " UTC";
}
