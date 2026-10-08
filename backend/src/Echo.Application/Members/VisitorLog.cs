using Microsoft.Extensions.Logging;

namespace Echo.Application.Members;

public static partial class VisitorLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor list: {Count} visitors returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Visitor found: {VisitorId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Visitor not found: {VisitorId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid visitorId);

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor created: {VisitorId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor updated: {VisitorId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Visitor update failed: {VisitorId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor deleted: {VisitorId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Visitor delete failed: {VisitorId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid visitorId);

    // --- Convert ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor converted: {VisitorId} → member {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Converted(
        ILogger logger,
        Guid congregationId,
        Guid visitorId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Visitor convert failed: {VisitorId} not found in congregation {CongregationId}"
    )]
    public static partial void ConvertNotFound(ILogger logger, Guid congregationId, Guid visitorId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Visitor convert failed: {VisitorId} already converted in congregation {CongregationId}"
    )]
    public static partial void AlreadyConverted(
        ILogger logger,
        Guid congregationId,
        Guid visitorId
    );

    // --- Summary ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Visitor summary: {TotalVisitors} visitors summarized for congregation {CongregationId}"
    )]
    public static partial void Summarized(ILogger logger, Guid congregationId, int totalVisitors);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Visitor error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
