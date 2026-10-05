using Microsoft.Extensions.Logging;

namespace Echo.Auth.Sessions;

public static partial class SessionLog
{
    // --- Login ---
    [LoggerMessage(Level = LogLevel.Warning, Message = "Login failed: user not found")]
    public static partial void LoginUserNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login failed: invalid credentials for user: {userId}"
    )]
    public static partial void LoginInvalidCredentials(ILogger logger, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Login failed: email not verified for user {UserId}"
    )]
    public static partial void LoginEmailNotVerified(ILogger logger, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Login succeeded: user {UserId} in congregation {CongregationId}"
    )]
    public static partial void LoginSucceeded(ILogger logger, Guid userId, Guid congregationId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Login error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void LoginFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );

    // --- Refresh ---
    [LoggerMessage(Level = LogLevel.Warning, Message = "Token refresh failed: {Reason}")]
    public static partial void TokenRefreshFailed(ILogger logger, string reason);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Token refresh failed: user not found for {UserId}"
    )]
    public static partial void TokenRefreshUserNotFound(ILogger logger, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Token refresh succeeded: user {UserId} in congregation {CongregationId}"
    )]
    public static partial void TokenRefreshSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Token refresh error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void TokenRefreshFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );

    // --- Revoke ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Token revoke succeeded: session terminated"
    )]
    public static partial void TokenRevoked(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Token revoke error: unexpected failure")]
    public static partial void TokenRevocationFailed(ILogger logger, Exception ex);

    // --- Logout all ---
    [LoggerMessage(Level = LogLevel.Warning, Message = "Logout all failed: user not found")]
    public static partial void LogoutOfAllSessionsFailed(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Logout all succeeded: all sessions for user {UserId} in congregation {CongregationId} terminated"
    )]
    public static partial void LogoutOfAllSessionsSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Logout all error: unexpected failure for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void LogoutAllFailed(
        ILogger logger,
        Exception ex,
        Guid userId,
        Guid congregationId
    );
}
