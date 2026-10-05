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
        Message = "Attendance create: service type {ServiceTypeId} found in congregation {CongregationId}"
    )]
    public static partial void CreateServiceTypeFound(
        ILogger logger,
        Guid congregationId,
        int serviceTypeId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance create failed: service type {ServiceTypeId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateServiceTypeNotFound(
        ILogger logger,
        Guid congregationId,
        int serviceTypeId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance create: person {PersonId} found in congregation {CongregationId}"
    )]
    public static partial void CreatePersonFound(
        ILogger logger,
        Guid congregationId,
        Guid personId
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

    // --- Summary ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance summary: {TotalPresent} present for congregation {CongregationId}"
    )]
    public static partial void Summarized(ILogger logger, Guid congregationId, int totalPresent);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
