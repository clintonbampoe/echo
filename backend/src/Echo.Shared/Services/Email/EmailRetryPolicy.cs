using System.Net;
using Resend;

namespace Echo.Shared.Services.Email;

public static class EmailRetryPolicy
{
    /// <summary>Waits before each retry; three entries means up to four attempts in total.</summary>
    public static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

    /// <summary>
    /// True for failures that can go away on their own: network errors and timeouts, rate limits
    /// and server errors. A rejected address or a bad API key will fail the same way again.
    /// </summary>
    public static bool IsRetryable(Exception exception) =>
        exception switch
        {
            ResendException resend => resend.IsTransient || IsRateLimitOrServerError(resend),
            HttpRequestException => true,
            TimeoutException => true,
            TaskCanceledException => true,
            _ => false,
        };

    // IsTransient looks at the error type only and misses plain 5xx responses.
    private static bool IsRateLimitOrServerError(ResendException exception) =>
        exception.StatusCode is HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError;
}
