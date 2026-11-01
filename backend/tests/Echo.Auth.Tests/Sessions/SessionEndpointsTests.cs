using Echo.Auth.Sessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Tests.Sessions;

[Trait("Category", "Unit")]
public class SessionEndpointsTests
{
    private static string? PolicyOf(string route)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<SessionService>(); // registered so the handler parameter is a service
        var app = builder.Build();
        using var instrumentation = new AuthInstrumentation();
        app.MapGroup("/api/v1").MapSessionEndpoints(instrumentation);

        return ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(e => e.RoutePattern.RawText == $"/api/v1/sessions{route}")
            .Metadata.GetMetadata<EnableRateLimitingAttribute>()
            ?.PolicyName;
    }

    // A church signs in together on a Sunday morning, so sign-in gets more room than the 5 a minute that
    // guards sign-up and password mail.
    [Fact]
    public void SigningIn_HasItsOwnLimit() => Assert.Equal("login", PolicyOf("/login"));

    // Renewing needs a refresh token nobody can guess, so there is nothing to protect but the server from
    // a flood, and a whole church renews through one address.
    [Theory]
    [InlineData("/refresh")]
    [InlineData("/revoke")]
    public void RenewingAndRevoking_HaveTheGenerousLimit(string route) =>
        Assert.Equal("session", PolicyOf(route));

    // Takes an email address and signs that account out everywhere, without a token, so it stays strict.
    [Fact]
    public void SigningEveryoneOut_StaysStrict() => Assert.Equal("auth", PolicyOf("/logout"));
}
