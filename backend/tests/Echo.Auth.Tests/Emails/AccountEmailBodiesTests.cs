using Echo.Auth.EmailVerifications;
using Echo.Auth.Invitations;
using Echo.Auth.Passwords;
using Echo.Domain.Users;

namespace Echo.Auth.Tests.Emails;

[Trait("Category", "Unit")]
public class AccountEmailBodiesTests
{
    private const string Site = "https://echo.test";

    [Fact]
    public void Verification_GreetsByFirstName_AndSaysVerifyAccount()
    {
        var body = new EmailVerificationMessageBody("Ama", Site + "/verify-email?token=t");

        Assert.Equal("Verify your Echo account", body.Subject);
        Assert.Contains("Hi Ama,", body.HtmlBody);
        Assert.Contains(">Verify account</a>", body.HtmlBody);
        Assert.Contains("https://echo.test/verify-email?token=t", body.HtmlBody);
        Assert.Contains("24 hours", body.HtmlBody);
    }

    [Fact]
    public void Verification_EscapesTheName()
    {
        var body = new EmailVerificationMessageBody("<b>Ama</b>", Site + "/verify-email?token=t");

        Assert.DoesNotContain("<b>Ama</b>", body.HtmlBody);
        Assert.Contains("&lt;b&gt;Ama&lt;/b&gt;", body.HtmlBody);
    }

    [Fact]
    public void PasswordReset_GreetsByFirstName_AndSaysHowLongTheLinkWorks()
    {
        var body = new PasswordResetEmailBody("Ama", Site + "/reset-password?token=t");

        Assert.Equal("Reset your Echo password", body.Subject);
        Assert.Contains("Hi Ama,", body.HtmlBody);
        Assert.Contains("https://echo.test/reset-password?token=t", body.HtmlBody);
        Assert.Contains("1 hour", body.HtmlBody);
    }

    [Fact]
    public void PasswordChanged_ShowsWhenAndWhichAccount_AndLinksToForgotPassword()
    {
        var body = new PasswordChangedEmailBody(
            "Ama",
            "ama@echo.test",
            new DateTime(2026, 11, 4, 11, 42, 0, DateTimeKind.Utc),
            Site + "/forgot"
        );

        Assert.Equal("Your Echo password was changed", body.Subject);
        Assert.Contains("4 November 2026, 11:42 UTC", body.HtmlBody);
        Assert.Contains("ama@echo.test", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/forgot\"", body.HtmlBody);
    }

    [Fact]
    public void Invitation_SaysTheRoleWithTheRightArticle()
    {
        var accountant = new InvitationEmailBody("Grace Chapel", UserRole.Accountant, "K7QX92MDPW4T", Site + "/join/c", null);
        var clerk = new InvitationEmailBody("Grace Chapel", UserRole.Clerk, "K7QX92MDPW4T", Site + "/join/c", null);

        Assert.Equal("You are invited to join Grace Chapel on Echo", accountant.Subject);
        Assert.Contains("as an Accountant", accountant.HtmlBody);
        Assert.Contains("as a Clerk", clerk.HtmlBody);
    }

    [Fact]
    public void Welcome_NamesTheChurchAndRole_AndLinksToTheApp()
    {
        var body = new WelcomeEmailBody("Kofi", "Grace Chapel", UserRole.Accountant, Site + "/dashboard");

        Assert.Equal("Welcome to Grace Chapel on Echo", body.Subject);
        Assert.Contains("Hi Kofi,", body.HtmlBody);
        Assert.Contains("as an Accountant", body.HtmlBody);
        Assert.Contains(">Accountant<", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/dashboard\"", body.HtmlBody);
        Assert.Contains("separate email to verify your account", body.HtmlBody);
    }

    [Fact]
    public void SetUpChurch_ListsTheThreeSteps_AndLinksToTheApp()
    {
        var body = new SetUpChurchEmailBody("Ama", "Grace Chapel", Site + "/dashboard");

        Assert.Equal("Set up Grace Chapel on Echo", body.Subject);
        Assert.Contains("Hi Ama,", body.HtmlBody);
        Assert.Contains("Add your members", body.HtmlBody);
        Assert.Contains("Create your first service session", body.HtmlBody);
        Assert.Contains("Invite your team", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/dashboard\"", body.HtmlBody);
    }

    [Fact]
    public void SetUpChurch_EscapesTheChurchName()
    {
        var body = new SetUpChurchEmailBody("Ama", "Grace <i>Chapel</i>", Site + "/dashboard");

        Assert.DoesNotContain("<i>Chapel</i>", body.HtmlBody);
        Assert.Contains("Grace &lt;i&gt;Chapel&lt;/i&gt;", body.HtmlBody);
    }
}
