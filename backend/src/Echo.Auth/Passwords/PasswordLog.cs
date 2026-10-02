using Microsoft.Extensions.Logging;

namespace Echo.Auth.Passwords;

public static partial class PasswordLog
{
    // --- Reset request ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password reset request failed: user not found"
    )]
    public static partial void PasswordResetRequestUserNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password reset request succeeded: email sent to user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetRequestSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Password reset request error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetRequestFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );

    // --- Reset ---
    [LoggerMessage(Level = LogLevel.Warning, Message = "Password reset failed: policy violation")]
    public static partial void PasswordResetFailedPolicyViolation(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Password reset failed: token not found")]
    public static partial void PasswordResetFailedTokenNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password reset failed: token expired for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetFailedTokenExpired(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Password reset failed: token already used for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetFailedTokenUsed(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password reset succeeded: password updated for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Password reset error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void PasswordResetFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );
}
