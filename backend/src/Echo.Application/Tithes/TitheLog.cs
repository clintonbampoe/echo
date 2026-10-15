using Microsoft.Extensions.Logging;

namespace Echo.Application.Tithes;

public static partial class TitheLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe list: {Count} tithes returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe list: {Count} tithes returned for member {memberId} in congregation {congregationId}"
    )]
    public static partial void ListByMemberId(
        ILogger logger,
        Guid memberId,
        Guid congregationId,
        int count
    );

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe found: {TitheId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe not found: {TitheId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid titheId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe create: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe create failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Duplicates ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe create refused: member {MemberId} already has a tithe for {Month} {Year} in congregation {CongregationId}"
    )]
    public static partial void CreateDuplicate(
        ILogger logger,
        Guid congregationId,
        Guid memberId,
        string month,
        int year
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe update refused: member {MemberId} already has a tithe for {Month} {Year} in congregation {CongregationId}"
    )]
    public static partial void UpdateDuplicate(
        ILogger logger,
        Guid congregationId,
        Guid memberId,
        string month,
        int year
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe created: {TitheId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe updated: {TitheId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe update failed: {TitheId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe deleted: {TitheId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe delete failed: {TitheId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid titheId);

    // --- Summary ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe summary: total {TotalCollected} collected for congregation {CongregationId}"
    )]
    public static partial void Summarized(
        ILogger logger,
        Guid congregationId,
        decimal totalCollected
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Tithe error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
