using Microsoft.AspNetCore.HttpOverrides;

namespace Echo.Api.Extensions;

public static class ForwardedHeadersConfiguration
{
    private const string TrustedNetworksKey = "ForwardedHeaders:TrustedNetworks";

    // Loopback plus the private ranges that Docker bridge networks are allocated from.
    // The API is not published in production, so only the reverse proxy can reach it.
    private static readonly string[] DefaultTrustedNetworks =
    [
        "127.0.0.0/8",
        "::1/128",
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
    ];

    /// <summary>
    /// Trusts X-Forwarded-For and X-Forwarded-Proto only from the networks in
    /// ForwardedHeaders:TrustedNetworks (CIDR list, e.g. ForwardedHeaders__TrustedNetworks__0).
    /// Blank entries are ignored; when none are left, the private ranges above are used.
    /// An entry that is not valid CIDR fails startup. Only the last hop is trusted, so a client
    /// cannot spoof its address by sending its own X-Forwarded-For.
    /// </summary>
    public static IServiceCollection ConfigureForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var configured = configuration
            .GetSection(TrustedNetworksKey)
            .Get<string[]>()
            ?.Where(network => !string.IsNullOrWhiteSpace(network))
            .ToArray();

        var trustedNetworks = (configured is { Length: > 0 } ? configured : DefaultTrustedNetworks)
            .Select(ParseNetwork)
            .ToList();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;

            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var network in trustedNetworks)
                options.KnownIPNetworks.Add(network);
        });

        return services;
    }

    private static System.Net.IPNetwork ParseNetwork(string value) =>
        System.Net.IPNetwork.TryParse(value.Trim(), out var network)
            ? network
            : throw new InvalidOperationException(
                $"Invalid {TrustedNetworksKey} entry '{value}'. Expected CIDR notation, e.g. 172.18.0.0/16."
            );
}
