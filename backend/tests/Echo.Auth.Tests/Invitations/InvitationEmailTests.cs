using Echo.Auth.Invitations;
using Echo.Domain.Users;
using Microsoft.Extensions.Options;
using Echo.Shared.Options.Frontend;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationEmailTests
{
    [Fact]
    public void TheJoinLink_PointsAtTheJoinScreenWithTheCode()
    {
        var links = new LinkBuilder(Options.Create(new FrontendOptions { BaseUrl = "https://echo.test" }));

        Assert.Equal("https://echo.test/join/K7QX-92MD-PW4T", links.BuildJoinLink("K7QX-92MD-PW4T"));
    }

    [Fact]
    public void TheEmail_HasTheLinkTheCodeAndTheExpiry_AndEscapesTheCongregationName()
    {
        var body = new InvitationEmailBody(
            "Grace <b>Chapel</b>",
            UserRole.Clerk,
            "K7QX-92MD-PW4T",
            "https://echo.test/join/K7QX-92MD-PW4T",
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Assert.Contains("https://echo.test/join/K7QX-92MD-PW4T", body.HtmlBody);
        Assert.Contains("K7QX-92MD-PW4T", body.HtmlBody);
        Assert.Contains("1 December 2026", body.HtmlBody);
        Assert.Contains("Grace &lt;b&gt;Chapel&lt;/b&gt;", body.HtmlBody);
        Assert.DoesNotContain("<b>Chapel</b>", body.HtmlBody);
    }

    [Fact]
    public void TheEmail_SaysSoWhenItNeverExpires()
    {
        var body = new InvitationEmailBody("Grace", UserRole.Member, "C", "https://x/join/C", null);

        Assert.Contains("does not expire", body.HtmlBody);
    }
}
