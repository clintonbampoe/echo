using Echo.Api.Extensions;
using Microsoft.AspNetCore.Http;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class SecurityHeadersTests
{
    private static IHeaderDictionary Headers(bool isDevelopment)
    {
        var headers = new HeaderDictionary();
        SecurityHeaders.Set(headers, isDevelopment);
        return headers;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Always_StopsSniffingFramingAndLeakingTheReferrer(bool isDevelopment)
    {
        var headers = Headers(isDevelopment);

        Assert.Equal("nosniff", headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", headers["X-Frame-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", headers["Referrer-Policy"]);
    }

    [Fact]
    public void OutsideDevelopment_AddsAStrictContentSecurityPolicy()
    {
        Assert.Equal(
            "default-src 'none'; frame-ancestors 'none'",
            Headers(isDevelopment: false)["Content-Security-Policy"]
        );
    }

    [Fact]
    public void InDevelopment_LeavesOutTheContentSecurityPolicySoTheApiDocsStillLoad()
    {
        Assert.False(Headers(isDevelopment: true).ContainsKey("Content-Security-Policy"));
    }

    [Fact]
    public void HstsIsNotSetByTheApi_ItBelongsToTheProxyThatTerminatesTls()
    {
        Assert.False(Headers(isDevelopment: false).ContainsKey("Strict-Transport-Security"));
    }
}
