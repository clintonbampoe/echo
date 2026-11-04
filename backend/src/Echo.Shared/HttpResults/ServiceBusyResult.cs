using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

/// <summary>
/// The server is busy with work only it can slow down (password checks, while many people sign in at once): 503, with
/// how long to wait in the Retry-After header. A part of a second is rounded up, so a caller never retries too soon.
/// </summary>
public class ServiceBusyResult(TimeSpan retryAfter) : IOperationResult
{
    public IResult ToResult()
    {
        var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
        var problem = TypedResults.Problem(
            detail: $"We are busy right now. Please try again in {seconds} seconds.",
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service Busy",
            type: ErrorTypes.ServiceBusy,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = "SERVICE_BUSY",
                ["retryAfterSeconds"] = seconds,
            }
        );
        return new WithRetryAfter(problem, seconds);
    }

    private sealed class WithRetryAfter(IResult inner, int seconds) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.Headers.RetryAfter = seconds.ToString();
            return inner.ExecuteAsync(httpContext);
        }
    }
}
