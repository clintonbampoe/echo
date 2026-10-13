using Echo.Shared.HttpResults;

namespace Echo.Shared.Tests.HttpResults;

[Trait("Category", "Unit")]
public class ErrorTypesTests
{
    [Fact]
    public void UseBaseUrl_BuildsTheTypeUrisFromTheBaseUrl_IgnoringATrailingSlash()
    {
        ErrorTypes.UseBaseUrl("https://api.echo.church/");

        Assert.Equal("https://api.echo.church/errors/bad-request", ErrorTypes.BadRequest);
        Assert.Equal("https://api.echo.church/errors/not-found", ErrorTypes.NotFound);
    }
}
