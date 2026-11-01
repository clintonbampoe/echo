using System.Net;
using Echo.Domain.Users;
using Echo.Shared.Models;

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
        $"""
            <html>
              <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>You're invited to {WebUtility.HtmlEncode(congregationName)}</h2>
                <p>You have been invited to join {WebUtility.HtmlEncode(congregationName)} on Echo as {role}.</p>
                <p>
                  <a href="{joinLink}"
                     style="display:inline-block; padding:5px 12px; background-color:#2563eb; color:#fff; text-decoration:none; border-radius:6px;">
                    Accept invitation
                  </a>
                </p>
                <p>Or enter this code on the join screen: <strong>{code}</strong></p>
                <p>{(expiresAt is null
                    ? "This invitation does not expire."
                    : $"This invitation expires on {expiresAt:d MMMM yyyy}.")} It can be used once.</p>
              </body>
            </html>
            """;
}
