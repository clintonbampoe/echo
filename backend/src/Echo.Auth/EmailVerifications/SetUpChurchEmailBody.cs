using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.EmailVerifications;

/// <summary>Sent when someone creates a church, to the admin who created it.</summary>
public class SetUpChurchEmailBody(string firstName, string congregationName, string dashboardLink)
    : IEmailContent
{
    public string Subject => $"Set up {congregationName} on Echo";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Your church",
                Heading = $"Set up {congregationName}",
                Preheader = "Three steps to get your church ready.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} {congregationName} is created and you are its admin. These three steps get it ready for your team.",
                ],
                Steps =
                [
                    new EmailStep(
                        "Add your members",
                        "Add the people in your congregation. Attendance and tithes are recorded against them."
                    ),
                    new EmailStep(
                        "Create your first service session",
                        "Set up a service so you can record attendance for it."
                    ),
                    new EmailStep(
                        "Invite your team",
                        "Send a link and a code to the people who will record attendance and tithes."
                    ),
                ],
                Button = new EmailButton("Start setting up", dashboardLink),
                Note = "We also sent a separate email to verify your account.",
                Reason = $"You are getting this because you created {congregationName} on Echo.",
            }
        );
}
