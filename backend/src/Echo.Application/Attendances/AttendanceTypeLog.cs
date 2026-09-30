using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceTypeLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type created: {AttendanceTypeId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, int attendanceTypeId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type updated: {AttendanceTypeId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, int attendanceTypeId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type deleted: {AttendanceTypeId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, int attendanceTypeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance type not found: {AttendanceTypeId} requested"
    )]
    public static partial void NotFound(ILogger logger, int attendanceTypeId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance type error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
