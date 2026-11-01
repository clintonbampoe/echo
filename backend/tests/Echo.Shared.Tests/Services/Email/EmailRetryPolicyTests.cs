using System.Net;
using Echo.Shared.Services.Email;
using Resend;

namespace Echo.Shared.Tests.Services.Email;

[Trait("Category", "Unit")]
public class EmailRetryPolicyTests
{
    private static ResendException Resend(HttpStatusCode status, ErrorType type) =>
        new(status, type, "boom", new ResendRateLimit());

    [Fact]
    public void Delays_AreTwoFourEightSeconds() =>
        Assert.Equal(
            [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8)],
            EmailRetryPolicy.Delays
        );

    [Fact]
    public void IsRetryable_NetworkError_True() =>
        Assert.True(EmailRetryPolicy.IsRetryable(new HttpRequestException("down")));

    [Fact]
    public void IsRetryable_ServerError_True() =>
        Assert.True(
            EmailRetryPolicy.IsRetryable(
                Resend(HttpStatusCode.InternalServerError, ErrorType.InternalServerError)
            )
        );

    [Fact]
    public void IsRetryable_RateLimited_True() =>
        Assert.True(
            EmailRetryPolicy.IsRetryable(
                Resend(HttpStatusCode.TooManyRequests, ErrorType.RateLimitExceeded)
            )
        );

    [Fact]
    public void IsRetryable_BadAddress4xx_False() =>
        Assert.False(
            EmailRetryPolicy.IsRetryable(
                Resend(HttpStatusCode.UnprocessableEntity, ErrorType.ValidationError)
            )
        );

    [Fact]
    public void IsRetryable_UnknownException_False() =>
        Assert.False(EmailRetryPolicy.IsRetryable(new InvalidOperationException("bug")));
}
