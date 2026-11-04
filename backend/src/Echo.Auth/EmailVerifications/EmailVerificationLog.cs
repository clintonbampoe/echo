using Microsoft.Extensions.Logging;

namespace Echo.Auth.EmailVerifications;

public static partial class EmailVerificationLog
{
    // --- Send ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification send failed: user not found"
    )]
    public static partial void SendVerificationFailedUserNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification send failed: rate limit active for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void SendVerificationFailedRateLimited(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email verification send succeeded: email sent to user {UserId} in congregation {CongregationId}"
    )]
    public static partial void SendVerificationSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Email verification send error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void SendVerificationFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );

    // --- Verify ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification failed: token not found"
    )]
    public static partial void VerifyEmailFailedTokenNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification failed: token expired for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void VerifyEmailFailedTokenExpired(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification failed: token already used for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void VerifyEmailFailedTokenUsed(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email verification failed: token invalidated for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void VerifyEmailFailedTokenInvalidated(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email verification succeeded: email verified for user {UserId} in congregation {CongregationId}. Token has been invalidated."
    )]
    public static partial void VerifyEmailSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Email verification error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void VerifyEmailFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );
}
