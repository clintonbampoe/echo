using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.EmailVerifications;

/// <summary>Sent when an account is verified, to everyone except the person who created the church.</summary>
public class WelcomeEmailBody(
    string firstName,
    string congregationName,
    string dashboardLink
) : IEmailContent
{
    public string Subject => "Your Echo account is verified";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Account",
                Heading = "Account verified",
                Preheader = "Here is where most churches start.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} welcome to {congregationName} on Echo. Here is where most churches start.",
                ],
                Steps =
                [
                    new EmailStep(
                        "Add your members",
                        "Build the list everything else is recorded against."
                    ),
                    new EmailStep(
                        "Create a service session",
                        "Set up Sunday service so check-ins have somewhere to go."
                    ),
                    new EmailStep(
                        "Record tithes",
                        "Log what was given and see it month by month."
                    ),
                ],
                Button = new EmailButton("Open Echo", dashboardLink),
                Reason = "You are getting this because your Echo account was just verified.",
            }
        );
}
