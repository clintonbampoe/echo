using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventAttendanceLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance created: {AttendanceId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance updated: {AttendanceId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance deleted: {AttendanceId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance not found: {AttendanceId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance creation failed: Event {EventId} not found"
    )]
    public static partial void EventNotFound(ILogger logger, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance creation failed: Member {MemberId} not found"
    )]
    public static partial void MemberNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Event attendance error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
