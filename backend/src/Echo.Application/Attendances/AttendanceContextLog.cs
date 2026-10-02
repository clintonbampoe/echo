using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceContextLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context list: {Count} contexts returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context search: {Count} results for query '{Query}' in congregation {CongregationId}"
    )]
    public static partial void Searched(
        ILogger logger,
        Guid congregationId,
        string query,
        int count
    );

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance context not found: {ContextId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int contextId);

    // --- Create ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context created: {ContextId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int contextId);

    // --- Update ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context updated: {ContextId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int contextId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance context update failed: {ContextId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, int contextId);

    // --- Delete ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context deleted: {ContextId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int contextId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance context delete failed: {ContextId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, int contextId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance context error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
