using Echo.Shared.Services.Email;

namespace Echo.Shared.Tests.Services.Email;

[Trait("Category", "Unit")]
public class EmailLayoutTests
{
    private static EmailPage Page(string heading = "Hello", string url = "https://echo.test/go") =>
        new()
        {
            Category = "Account",
            Heading = heading,
            Preheader = "Short preview",
            Paragraphs = ["First line."],
            Button = new EmailButton("Go", url),
            Reason = "Because.",
            SiteUrl = "https://echo.test",
        };

    [Fact]
    public void Greeting_UsesTheName_OrFallsBack()
    {
        Assert.Equal("Hi Ama,", EmailLayout.Greeting(" Ama "));
        Assert.Equal("Hi there,", EmailLayout.Greeting(null));
        Assert.Equal("Hi there,", EmailLayout.Greeting("  "));
    }

    [Fact]
    public void Render_HasTheHeadingTheButtonTheReasonAndThePreheader()
    {
        var html = EmailLayout.Render(Page());

        Assert.Contains(">Hello</td>", html);
        Assert.Contains("""href="https://echo.test/go" """, html);
        Assert.Contains(">Go</a>", html);
        Assert.Contains("Because.", html);
        Assert.Contains("Short preview", html);
    }

    [Fact]
    public void Render_EncodesEveryValue()
    {
        var html = EmailLayout.Render(
            Page(heading: "<script>alert(1)</script>", url: "https://echo.test/go?a=1&b=\"2\"")
        );

        Assert.DoesNotContain("<script>alert", html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.Contains("a=1&amp;b=&quot;2&quot;", html);
    }

    [Fact]
    public void Render_LoadsBothLogosFromTheSite_WithoutADoubleSlash()
    {
        var page = new EmailPage
        {
            Category = "Account",
            Heading = "Hello",
            Preheader = "p",
            Paragraphs = ["x"],
            Button = new EmailButton("Go", "https://echo.test/go"),
            Reason = "r",
            SiteUrl = "https://echo.test/",
        };

        var html = EmailLayout.Render(page);

        Assert.Contains("https://echo.test/email/echo-mark.png", html);
        Assert.Contains("https://echo.test/email/echo-mark-light.png", html);
        Assert.DoesNotContain("echo.test//email", html);
    }

    [Fact]
    public void Render_LeavesOutWhatThePageDoesNotUse()
    {
        var html = EmailLayout.Render(Page());

        Assert.DoesNotContain("Button not working?", html);
        Assert.DoesNotContain("class=\"code ", html);
    }

    [Fact]
    public void Render_ShowsTheLinkAsText_WhenAsked()
    {
        var page = new EmailPage
        {
            Category = "Account",
            Heading = "Hello",
            Preheader = "p",
            Paragraphs = ["x"],
            Button = new EmailButton("Go", "https://echo.test/go"),
            Reason = "r",
            SiteUrl = "https://echo.test",
            ShowLinkFallback = true,
        };

        Assert.Contains("Button not working?", EmailLayout.Render(page));
    }

    [Fact]
    public void Render_ShowsRowsStepsCodeAndNote()
    {
        var page = new EmailPage
        {
            Category = "Invitation",
            Heading = "Hello",
            Preheader = "p",
            Paragraphs = ["x"],
            Button = new EmailButton("Go", "https://echo.test/go"),
            Reason = "r",
            SiteUrl = "https://echo.test",
            Rows = [new EmailRow("Church", "Grace Chapel")],
            Steps = [new EmailStep("Add your members", "Start the list.")],
            Code = new EmailCode("Enter this code", "K7QX-92MD-PW4T"),
            Note = "Works for 1 hour.",
        };

        var html = EmailLayout.Render(page);

        Assert.Contains("Grace Chapel", html);
        Assert.Contains("Add your members", html);
        Assert.Contains("K7QX-92MD-PW4T", html);
        Assert.Contains("Works for 1 hour.", html);
    }
}
