using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance list: {Count} records returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance found: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance not found: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid attendanceId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance create: context {ContextId} found in congregation {CongregationId}"
    )]
    public static partial void CreateContextFound(
        ILogger logger,
        Guid congregationId,
        int contextId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance create failed: context {ContextId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateContextNotFound(
        ILogger logger,
        Guid congregationId,
        int contextId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance update: context {ContextId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateContextFound(
        ILogger logger,
        Guid congregationId,
        int contextId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance update failed: context {ContextId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateContextNotFound(
        ILogger logger,
        Guid congregationId,
        int contextId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance created: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance updated: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance update failed: {AttendanceId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid attendanceId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance deleted: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance delete failed: {AttendanceId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid attendanceId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
