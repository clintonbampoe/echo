using Echo.Domain.Users;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;

namespace Echo.Auth.EmailVerifications;

/// <summary>Sent when someone signs up to join a church with an invitation code.</summary>
public class WelcomeEmailBody(
    string firstName,
    string congregationName,
    UserRole role,
    string dashboardLink
) : IEmailContent
{
    public string Subject => $"Welcome to {congregationName} on Echo";

    public string HtmlBody =>
        EmailLayout.Render(
            new EmailPage
            {
                Category = "Welcome",
                Heading = $"Welcome to {congregationName}",
                Preheader = $"You joined as {AOrAn} {role}.",
                Paragraphs =
                [
                    $"{EmailLayout.Greeting(firstName)} you have joined {congregationName} on Echo as {AOrAn} {role}. Open Echo to get started.",
                ],
                Rows =
                [
                    new EmailRow("Church", congregationName),
                    new EmailRow("Your role", role.ToString()),
                ],
                Button = new EmailButton("Open Echo", dashboardLink),
                Note = "We also sent a separate email to verify your account.",
                Reason = "You are getting this because you signed up for Echo with this email address.",
            }
        );

    private string AOrAn => "AEIOU".Contains(role.ToString()[0]) ? "an" : "a";
}
