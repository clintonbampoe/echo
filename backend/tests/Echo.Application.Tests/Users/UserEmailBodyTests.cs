using Echo.Application.Users.Emails;
using Echo.Domain.Users;

namespace Echo.Application.Tests.Users;

[Trait("Category", "Unit")]
public class UserEmailBodyTests
{
    private static readonly DateTime At = new(2026, 11, 8, 15, 4, 0, DateTimeKind.Utc);

    private static RoleChangedEmailBody Role(UserRole from, UserRole to) =>
        new("Ama", "Grace Chapel", from, to, "Kwabena Ansah", At, "https://echo.test/dashboard");

    [Fact]
    public void RaisedRole_SaysMoreAccess_AndKeepsTheOpenButton()
    {
        var body = Role(UserRole.Accountant, UserRole.Admin);

        Assert.Equal("You are now an Admin at Grace Chapel", body.Subject);
        Assert.Contains("more access", body.HtmlBody);
        Assert.Contains("Previous role", body.HtmlBody);
        Assert.Contains("8 November 2026, 15:04 UTC", body.HtmlBody);
        Assert.Contains("https://echo.test/dashboard", body.HtmlBody);
    }

    [Fact]
    public void LoweredRole_SaysLessAccess()
    {
        var body = Role(UserRole.Admin, UserRole.Clerk);

        Assert.Equal("Your role at Grace Chapel changed to Clerk", body.Subject);
        Assert.Contains("less access", body.HtmlBody);
    }

    [Fact]
    public void DisabledAndRemoved_HaveNoButton()
    {
        var disabled = new AccountDisabledEmailBody("Ama", "Grace Chapel", "ama@echo.test", At);
        var removed = new AccountRemovedEmailBody("Ama", "Grace Chapel", "ama@echo.test", At);

        Assert.Contains("Your account is disabled", disabled.HtmlBody);
        Assert.Contains("Your account was removed", removed.HtmlBody);
        Assert.DoesNotContain("class=\"btn-table\"", disabled.HtmlBody);
        Assert.DoesNotContain("class=\"btn-table\"", removed.HtmlBody);
    }
}
