using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance list: {Count} records returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

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

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance create failed: session {SessionId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateSessionNotFound(
        ILogger logger,
        Guid congregationId,
        Guid sessionId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance create failed: person {PersonId} not found in congregation {CongregationId}"
    )]
    public static partial void CreatePersonNotFound(
        ILogger logger,
        Guid congregationId,
        Guid personId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Bulk attendance for session {SessionId} in congregation {CongregationId} hit a concurrent duplicate; nothing was saved"
    )]
    public static partial void BulkConflict(ILogger logger, Guid congregationId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Bulk attendance for session {SessionId} in congregation {CongregationId}: {Created} created, {Skipped} skipped"
    )]
    public static partial void BulkCreated(
        ILogger logger,
        Guid congregationId,
        Guid sessionId,
        int created,
        int skipped
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance create rejected: person {PersonId} already marked present at session {SessionId} in congregation {CongregationId}"
    )]
    public static partial void DuplicateRecord(
        ILogger logger,
        Guid congregationId,
        Guid sessionId,
        Guid personId
    );

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

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance summary: {TotalPresent} present for congregation {CongregationId}"
    )]
    public static partial void Summarized(ILogger logger, Guid congregationId, int totalPresent);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
