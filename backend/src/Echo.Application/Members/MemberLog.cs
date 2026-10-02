using Microsoft.Extensions.Logging;

namespace Echo.Application.Members;

public static partial class MemberLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member list: {Count} members returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Member found: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Member not found: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid memberId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Member create failed: email already taken in congregation {CongregationId}"
    )]
    public static partial void CreateEmailTaken(ILogger logger, Guid congregationId);

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Member update failed: email already taken in congregation {CongregationId}"
    )]
    public static partial void UpdateEmailTaken(ILogger logger, Guid congregationId);

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member created: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member updated: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Member update failed: {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member deleted: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Member delete failed: {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid memberId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Member error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
