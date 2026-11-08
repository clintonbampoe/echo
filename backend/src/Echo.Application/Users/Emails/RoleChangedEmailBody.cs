using System.Globalization;
using Echo.Domain.Users;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Application.Users.Emails;

/// <summary>
/// Sent to a person when an admin changes their role. It says whether the role went up or down,
/// because "you have more access" and "you have less access" are different news.
/// </summary>
public class RoleChangedEmailBody(
    string firstName,
    string congregationName,
    UserRole previousRole,
    UserRole newRole,
    string changedBy,
    DateTime changedAtUtc,
    string dashboardLink
) : IEmailContent
{
    // Roles are declared from the most access to the least, so a smaller value is more access.
    private bool Raised => newRole < previousRole;

    public string Subject =>
        Raised
            ? $"You are now {AOrAn(newRole)} {newRole} at {congregationName}"
            : $"Your role at {congregationName} changed to {newRole}";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Role",
                Heading = Raised ? $"You are now {AOrAn(newRole)} {newRole}" : $"Your role is now {newRole}",
                Preheader = "Your role on Echo was changed.",
                Paragraphs =
                [
                    Raised
                        ? $"{EmailLayout.Greeting(firstName)} {changedBy} changed your role at {congregationName} on Echo. You now have more access than before."
                        : $"{EmailLayout.Greeting(firstName)} {changedBy} changed your role at {congregationName} on Echo. You now have less access than before, so some parts of Echo are no longer available to you.",
                ],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Previous role", previousRole.ToString()),
                    new EmailRow("New role", newRole.ToString()),
                    new EmailRow("Changed by", changedBy),
                    new EmailRow("When", When),
                ],
                Button = new EmailButton("Open Echo", dashboardLink),
                Note = "The change reaches your account within a few minutes. To see it right away, sign out and sign in again.",
                Reason = "We send this email every time your role changes. If this looks wrong, ask an admin at "
                    + congregationName
                    + ".",
            }
        );

    private string When =>
        changedAtUtc.ToString("d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture) + " UTC";

    private static string AOrAn(UserRole role) => "AEIOU".Contains(role.ToString()[0]) ? "an" : "a";
}
