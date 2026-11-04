using System.Globalization;
using Echo.Domain.Users;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.Invitations;

public class InvitationEmailBody(
    string congregationName,
    UserRole role,
    string code,
    string joinLink,
    DateTime? expiresAt
) : IEmailContent
{
    public string Subject => $"You're invited to join {congregationName} on Echo";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Invitation",
                Heading = $"Join {congregationName} on Echo",
                Preheader = $"Join as {RoleLabel}. Use the button or the code.",
                Paragraphs = [$"You have been invited to join {congregationName} on Echo."],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Your role", RoleLabel),
                ],
                Button = new EmailButton("Accept invitation", joinLink),
                Code = new EmailCode("Or enter this code on the join screen", code),
                Note = $"{Expiry} It can be used once.",
                Reason = $"You are getting this because an admin at {congregationName} invited this address. If you were not expecting it, ignore this message.",
            }
        );

    private string RoleLabel => role.ToString();

    private string Expiry =>
        expiresAt is null
            ? "This invitation does not expire."
            : $"This invitation expires on {expiresAt.Value.ToString("d MMMM yyyy", CultureInfo.InvariantCulture)}.";
}
