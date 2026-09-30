using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance created: {AttendanceId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance updated: {AttendanceId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance deleted: {AttendanceId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance not found: {AttendanceId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance creation failed: Context {ContextId} not found"
    )]
    public static partial void ContextNotFound(ILogger logger, int contextId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
