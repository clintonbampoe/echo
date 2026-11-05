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
    public void Render_LoadsNoImages_SoNothingDependsOnTheNetwork()
    {
        var html = EmailLayout.Render(Page());

        Assert.DoesNotContain("<img", html);
        Assert.Contains(">Echo</span>", html);
    }

    [Fact]
    public void Render_PutsTheWordmarkCategoryAndHeadingInTheHeaderBlock()
    {
        var html = EmailLayout.Render(Page(heading: "Verify your account"));

        var hero = html.IndexOf("c-hero", StringComparison.Ordinal);
        Assert.True(hero > 0);
        Assert.True(html.IndexOf(">Echo</span>", StringComparison.Ordinal) > hero);
        Assert.True(html.IndexOf(">Account</td>", StringComparison.Ordinal) > hero);
        Assert.True(html.IndexOf(">Verify your account</td>", StringComparison.Ordinal) > hero);
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
