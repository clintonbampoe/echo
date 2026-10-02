using Microsoft.Extensions.Logging;

namespace Echo.Auth.Registrations;

public static partial class RegistrationLog
{
    // --- Congregation registration ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Congregation registration failed: policy violation"
    )]
    public static partial void CongregationRegistrationFailedPolicyViolation(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Congregation registration failed: email already in use"
    )]
    public static partial void CongregationRegistrationFailedEmailTaken(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Congregation registration succeeded: congregation {CongregationId} created with admin {UserId}"
    )]
    public static partial void CongregationRegistrationSucceeded(
        ILogger logger,
        Guid congregationId,
        Guid userId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Congregation registration error: unexpected failure for congregation {CongregationId} user {UserId}"
    )]
    public static partial void CongregationRegistrationFailed(
        ILogger logger,
        Exception ex,
        Guid congregationId,
        Guid userId
    );

    // --- User registration ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User registration failed: invalid invitation"
    )]
    public static partial void UserRegistrationFailedInvalidInvitation(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User registration failed: policy violation for congregation {CongregationId}"
    )]
    public static partial void UserRegistrationFailedPolicyViolation(
        ILogger logger,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User registration failed: email already in use for congregation {CongregationId}"
    )]
    public static partial void UserRegistrationFailedEmailTaken(
        ILogger logger,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User registration succeeded: user {UserId} registered in congregation {CongregationId}"
    )]
    public static partial void UserRegistrationSucceeded(
        ILogger logger,
        Guid userId,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "User registration error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void UserRegistrationFailed(
        ILogger logger,
        Exception ex,
        Guid congregationId
    );
}
