using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceContextLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context created: {AttendanceContextId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, int attendanceContextId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context updated: {AttendanceContextId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, int attendanceContextId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance context deleted: {AttendanceContextId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, int attendanceContextId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance context not found: {AttendanceContextId} requested"
    )]
    public static partial void NotFound(ILogger logger, int attendanceContextId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance context error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
