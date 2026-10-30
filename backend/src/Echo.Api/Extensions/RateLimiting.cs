using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Echo.Api.Extensions;

public static class AddRateLimitingExtensions
{
    // A signed-in person is counted on their own, so one busy page (the dashboard asks for about twenty things)
    // does not eat the allowance of everyone behind the same proxy or office address, and does not lock its own
    // user out after four or five page changes. Anyone not signed in is counted by address, as before.
    internal const int SignedInPerMinute = 600;
    internal const int AnonymousPerMinute = 100;

    internal static RateLimitPartition<string> GlobalPartition(HttpContext context)
    {
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var signedIn = context.User.Identity?.IsAuthenticated == true && userId is not null;
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: signedIn
                ? $"user:{userId}"
                : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = signedIn ? SignedInPerMinute : AnonymousPerMinute,
                QueueLimit = 0,
            }
        );
    }

    public static IServiceCollection ConfigureRateLimits(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(GlobalPartition);

            options.AddPolicy(
                "auth",
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 5,
                            QueueLimit = 0,
                        }
                    )
            );

            options.AddPolicy(
                "search",
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 60,
                            QueueLimit = 0,
                        }
                    )
            );
        });

        return services;
    }
}
