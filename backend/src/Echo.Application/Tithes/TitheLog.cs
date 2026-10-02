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

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe update: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe update failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
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

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Tithe error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
