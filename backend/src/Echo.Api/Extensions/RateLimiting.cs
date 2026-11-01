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

    // Search runs on every keystroke of a type-ahead box, so it gets its own allowance, counted the same way:
    // twenty staff in one office are twenty people, not one address. The allowance is the ordinary one: search is
    // cheap now (five rows), and the front end debounces it.
    internal const int SearchPerMinute = SignedInPerMinute;

    // Counted by address, because the caller has no usable token yet. A church shares one address, so each
    // allowance is sized for a whole office, by how much there is to guess:
    //   auth     sign-up, password mail, verification, invitation codes, sign out everywhere: the abuse targets
    //   login    a church signing in together; the password is checked with bcrypt, which is slow anyway
    //   session  renewing and revoking with a refresh token nobody can guess, so only floods matter
    internal const int AuthPerMinute = 5;
    internal const int LoginPerMinute = 30;
    internal const int SessionPerMinute = 120;

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

    internal static RateLimitPartition<string> SearchPartition(HttpContext context)
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
                PermitLimit = SearchPerMinute,
                QueueLimit = 0,
            }
        );
    }

    internal static RateLimitPartition<string> AddressPartition(
        HttpContext context,
        int perMinute
    ) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = perMinute,
                QueueLimit = 0,
            }
        );

    public static IServiceCollection ConfigureRateLimits(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                GlobalPartition
            );

            options.AddPolicy("auth", context => AddressPartition(context, AuthPerMinute));
            options.AddPolicy("login", context => AddressPartition(context, LoginPerMinute));
            options.AddPolicy("session", context => AddressPartition(context, SessionPerMinute));
            options.AddPolicy("search", SearchPartition);
        });

        return services;
    }
}
