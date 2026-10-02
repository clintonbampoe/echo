using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceTypeLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type list: {Count} types returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type search: {Count} results for query '{Query}' in congregation {CongregationId}"
    )]
    public static partial void Searched(
        ILogger logger,
        Guid congregationId,
        string query,
        int count
    );

    // --- Get by id ---

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type found: {TypeId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, int typeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance type not found: {TypeId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int typeId);

    // --- Create ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type created: {TypeId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int typeId);

    // --- Update ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type updated: {TypeId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int typeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance type update failed: {TypeId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, int typeId);

    // --- Delete ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Attendance type deleted: {TypeId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int typeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Attendance type delete failed: {TypeId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, int typeId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Attendance type error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
