using System.Net;
using Echo.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class ForwardedHeadersTests
{
    private const string NginxOnDockerNetwork = "172.18.0.5";
    private const string RealClient = "203.0.113.7";

    private static ForwardedHeadersOptions BuildOptions(
        Dictionary<string, string?>? settings = null
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings ?? new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();
        services.ConfigureForwardedHeaders(configuration);

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
    }

    private static async Task<HttpContext> Send(
        ForwardedHeadersOptions options,
        string peerIp,
        string? forwardedFor,
        string? forwardedProto = null
    )
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(peerIp);
        if (forwardedFor is not null)
            context.Request.Headers["X-Forwarded-For"] = forwardedFor;
        if (forwardedProto is not null)
            context.Request.Headers["X-Forwarded-Proto"] = forwardedProto;

        var middleware = new ForwardedHeadersMiddleware(
            _ => Task.CompletedTask,
            NullLoggerFactory.Instance,
            Options.Create(options)
        );
        await middleware.Invoke(context);

        return context;
    }

    [Fact]
    public async Task Default_ProxyOnDockerNetwork_ReplacesProxyIpWithClientIp()
    {
        var context = await Send(BuildOptions(), NginxOnDockerNetwork, RealClient);

        Assert.Equal(IPAddress.Parse(RealClient), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Default_ClientSpoofsLeftmostEntry_UsesAddressNginxAppended()
    {
        // nginx's $proxy_add_x_forwarded_for appends the real peer to what the client sent.
        var context = await Send(BuildOptions(), NginxOnDockerNetwork, $"1.2.3.4, {RealClient}");

        Assert.Equal(IPAddress.Parse(RealClient), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Default_PeerOutsideTrustedNetworks_IgnoresForwardedHeader()
    {
        var context = await Send(BuildOptions(), "198.51.100.9", "1.2.3.4");

        Assert.Equal(IPAddress.Parse("198.51.100.9"), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Default_ProxyOnDockerNetwork_AppliesForwardedProto()
    {
        var context = await Send(BuildOptions(), NginxOnDockerNetwork, RealClient, "https");

        Assert.Equal("https", context.Request.Scheme);
    }

    [Fact]
    public async Task ConfiguredNetworks_ReplaceTheDefaults()
    {
        var options = BuildOptions(
            new Dictionary<string, string?>
            {
                ["ForwardedHeaders:TrustedNetworks:0"] = "198.51.100.0/24",
            }
        );

        var fromDockerNetwork = await Send(options, NginxOnDockerNetwork, RealClient);
        var fromConfiguredNetwork = await Send(options, "198.51.100.9", RealClient);

        Assert.Equal(
            IPAddress.Parse(NginxOnDockerNetwork),
            fromDockerNetwork.Connection.RemoteIpAddress
        );
        Assert.Equal(
            IPAddress.Parse(RealClient),
            fromConfiguredNetwork.Connection.RemoteIpAddress
        );
    }

    [Fact]
    public async Task Default_Ipv4MappedProxyAddress_ReplacesProxyIpWithClientIp()
    {
        // Kestrel on a dual-stack socket reports IPv4 peers as ::ffff:a.b.c.d.
        var context = await Send(BuildOptions(), $"::ffff:{NginxOnDockerNetwork}", RealClient);

        Assert.Equal(IPAddress.Parse(RealClient), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task BlankConfiguredNetwork_FallsBackToTheDefaults()
    {
        // An unset ${VAR} in docker-compose arrives as an empty string.
        var options = BuildOptions(
            new Dictionary<string, string?> { ["ForwardedHeaders:TrustedNetworks:0"] = "" }
        );

        var context = await Send(options, NginxOnDockerNetwork, RealClient);

        Assert.Equal(IPAddress.Parse(RealClient), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public void InvalidConfiguredNetwork_FailsWithAMessageNamingTheSettingAndValue()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ForwardedHeaders:TrustedNetworks:0"] = "172.18.0.5",
        };

        var ex = Assert.Throws<InvalidOperationException>(() => BuildOptions(settings));

        Assert.Contains("ForwardedHeaders:TrustedNetworks", ex.Message);
        Assert.Contains("172.18.0.5", ex.Message);
    }
}
