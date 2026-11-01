using Microsoft.Extensions.Logging;

namespace Echo.Shared.Services.Email;

public static partial class EmailLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Email queue is full: dropped {Kind} email for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void QueueFull(
        ILogger logger,
        string kind,
        Guid? userId,
        Guid? congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Sent {Kind} email for user {UserId} in congregation {CongregationId}"
    )]
    public static partial void Sent(ILogger logger, string kind, Guid? userId, Guid? congregationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Sending {Kind} email failed (attempt {Attempt}), retrying in {Delay}"
    )]
    public static partial void Retrying(
        ILogger logger,
        Exception ex,
        string kind,
        int attempt,
        TimeSpan delay
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Giving up on {Kind} email for user {UserId} in congregation {CongregationId} after {Attempts} attempt(s)"
    )]
    public static partial void GaveUp(
        ILogger logger,
        Exception ex,
        string kind,
        Guid? userId,
        Guid? congregationId,
        int attempts
    );
}
