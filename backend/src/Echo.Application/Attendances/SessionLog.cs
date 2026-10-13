using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class SessionLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session list: {Count} sessions returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session found: {SessionId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session not found: {SessionId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session create failed: label {LabelId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateLabelNotFound(
        ILogger logger,
        Guid congregationId,
        int labelId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session created: {SessionId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session updated: {SessionId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session update failed: {SessionId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session deleted: {SessionId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session delete failed: {SessionId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session summary: {TotalSessions} sessions for congregation {CongregationId}"
    )]
    public static partial void Summarized(ILogger logger, Guid congregationId, int totalSessions);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Session error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
