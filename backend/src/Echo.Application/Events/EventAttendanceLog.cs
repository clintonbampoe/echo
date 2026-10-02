using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventAttendanceLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance list: {Count} records returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance found: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance not found: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid attendanceId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance create: event {EventId} found in congregation {CongregationId}"
    )]
    public static partial void CreateEventFound(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance create failed: event {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateEventNotFound(
        ILogger logger,
        Guid congregationId,
        Guid eventId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance create: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance create failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance update: event {EventId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateEventFound(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance update failed: event {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateEventNotFound(
        ILogger logger,
        Guid congregationId,
        Guid eventId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance update: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance update failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance created: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance updated: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance update failed: {AttendanceId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid attendanceId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event attendance deleted: {AttendanceId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid attendanceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event attendance delete failed: {AttendanceId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid attendanceId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Event attendance error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
