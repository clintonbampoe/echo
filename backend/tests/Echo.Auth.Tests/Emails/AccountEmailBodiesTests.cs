using Echo.Auth.EmailVerifications;
using Echo.Auth.Passwords;

namespace Echo.Auth.Tests.Emails;

[Trait("Category", "Unit")]
public class AccountEmailBodiesTests
{
    private const string Site = "https://echo.test";

    [Fact]
    public void Verification_GreetsByFirstName_AndSaysVerifyAccount()
    {
        var body = new EmailVerificationMessageBody("Ama", Site + "/verify-email?token=t", Site);

        Assert.Equal("Verify your Echo account", body.Subject);
        Assert.Contains("Hi Ama,", body.HtmlBody);
        Assert.Contains(">Verify account</a>", body.HtmlBody);
        Assert.Contains("https://echo.test/verify-email?token=t", body.HtmlBody);
        Assert.Contains("24 hours", body.HtmlBody);
    }

    [Fact]
    public void Verification_EscapesTheName()
    {
        var body = new EmailVerificationMessageBody("<b>Ama</b>", Site + "/verify-email?token=t", Site);

        Assert.DoesNotContain("<b>Ama</b>", body.HtmlBody);
        Assert.Contains("&lt;b&gt;Ama&lt;/b&gt;", body.HtmlBody);
    }

    [Fact]
    public void PasswordReset_GreetsByFirstName_AndSaysHowLongTheLinkWorks()
    {
        var body = new PasswordResetEmailBody("Ama", Site + "/reset-password?token=t", Site);

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
            Site + "/forgot",
            Site
        );

        Assert.Equal("Your Echo password was changed", body.Subject);
        Assert.Contains("4 November 2026, 11:42 UTC", body.HtmlBody);
        Assert.Contains("ama@echo.test", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/forgot\"", body.HtmlBody);
    }

    [Fact]
    public void Welcome_NamesTheChurch_AndLinksToTheApp()
    {
        var body = new WelcomeEmailBody("Kofi", "Grace Chapel", Site + "/dashboard", Site);

        Assert.Equal("Your Echo account is verified", body.Subject);
        Assert.Contains("Hi Kofi,", body.HtmlBody);
        Assert.Contains("Grace Chapel", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/dashboard\"", body.HtmlBody);
        Assert.Contains("Add your members", body.HtmlBody);
    }

    [Fact]
    public void ChurchReady_NamesTheChurch_AndPointsAtInvitations()
    {
        var body = new ChurchReadyEmailBody("Ama", "Grace Chapel", Site + "/invitations", Site);

        Assert.Equal("Grace Chapel is ready on Echo", body.Subject);
        Assert.Contains("Grace Chapel is ready", body.HtmlBody);
        Assert.Contains(">Admin<", body.HtmlBody);
        Assert.Contains("href=\"https://echo.test/invitations\"", body.HtmlBody);
    }

    [Fact]
    public void ChurchReady_EscapesTheChurchName()
    {
        var body = new ChurchReadyEmailBody("Ama", "Grace <i>Chapel</i>", Site + "/invitations", Site);

        Assert.DoesNotContain("<i>Chapel</i>", body.HtmlBody);
        Assert.Contains("Grace &lt;i&gt;Chapel&lt;/i&gt;", body.HtmlBody);
    }
}
