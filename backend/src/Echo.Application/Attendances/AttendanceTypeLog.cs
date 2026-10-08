using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class AttendanceTypeLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Service type list: {Count} types returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Service type search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Service type found: {ServiceTypeId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, int serviceTypeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service type not found: {ServiceTypeId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int serviceTypeId);

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Service type created: {ServiceTypeId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int serviceTypeId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Service type updated: {ServiceTypeId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int serviceTypeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service type update failed: {ServiceTypeId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        int serviceTypeId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Service type deleted: {ServiceTypeId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int serviceTypeId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service type delete failed: {ServiceTypeId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        int serviceTypeId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Service type error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
