using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.EmailVerifications;

/// <summary>Sent in place of the welcome email when the person who created the church verifies.</summary>
public class ChurchReadyEmailBody(
    string firstName,
    string congregationName,
    string invitationsLink,
    string siteUrl
) : IEmailContent
{
    public string Subject => $"{congregationName} is ready on Echo";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Your church",
                Heading = $"{congregationName} is ready",
                Preheader = "You are the admin. Invite your team next.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} your account is verified and {congregationName} is set up on Echo.",
                ],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Your role", "Admin"),
                ],
                Section = new EmailSection(
                    "Next, invite your team",
                    "Invite the people who will record attendance, tithes and members. Each gets a link and a code."
                ),
                Button = new EmailButton("Invite your team", invitationsLink),
                Reason = $"You are getting this because you created {congregationName} on Echo and verified your account.",
                SiteUrl = siteUrl,
            }
        );
}
