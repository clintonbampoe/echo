using System.Net;
using Echo.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Api.Tests.Extensions;

// Runs the pipeline Program.cs uses (UseApiPipeline) with the real forwarded-headers and
// rate-limit setup, so moving or removing a middleware fails a test.
[Trait("Category", "Unit")]
public class PipelineOrderTests
{
    private const string TrustedProxy = "172.18.0.5";
    private const string UntrustedPeer = "203.0.113.9";

    private static async Task<WebApplication> Start(string peerIp)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["Cors:AllowedOrigins"] = "http://localhost" }
        );
        builder
            .Services.ConfigureForwardedHeaders(builder.Configuration)
            .ConfigureRateLimits()
            .ConfigureCors(builder.Configuration);
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Stands in for the socket: TestServer has no peer address of its own.
        app.Use(
            (context, next) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(peerIp);
                return next();
            }
        );
        app.UseApiPipeline();
        app.MapGet("/ping", () => "pong").RequireRateLimiting("auth");
        await app.StartAsync();
        return app;
    }

    private static async Task<HttpStatusCode> Ping(HttpClient client, string forwardedFor)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        var res = await client.SendAsync(request, TestContext.Current.CancellationToken);
        return res.StatusCode;
    }

    // The "auth" policy allows 5 requests a minute per client.
    private static async Task UseUpTheQuota(HttpClient client, string forwardedFor)
    {
        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.OK, await Ping(client, forwardedFor));
    }

    [Fact]
    public async Task TrustedProxy_EachForwardedClientGetsItsOwnRateLimitBucket()
    {
        await using var app = await Start(TrustedProxy);
        using var client = app.GetTestClient();

        await UseUpTheQuota(client, "198.51.100.1");

        Assert.Equal(HttpStatusCode.TooManyRequests, await Ping(client, "198.51.100.1"));
        Assert.Equal(HttpStatusCode.OK, await Ping(client, "198.51.100.2"));
    }

    [Fact]
    public async Task UntrustedPeer_CannotPickItsOwnRateLimitBucket()
    {
        await using var app = await Start(UntrustedPeer);
        using var client = app.GetTestClient();

        await UseUpTheQuota(client, "198.51.100.1");

        Assert.Equal(HttpStatusCode.TooManyRequests, await Ping(client, "198.51.100.2"));
    }
}
