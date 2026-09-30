using Microsoft.Extensions.Logging;

namespace Echo.Application.Tithes;

public static partial class TitheLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe created: {TitheId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tithe updated: {TitheId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid titheId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tithe deleted: {TitheId} soft-deleted")]
    public static partial void Deleted(ILogger logger, Guid titheId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tithe not found: {TitheId} requested")]
    public static partial void NotFound(ILogger logger, Guid titheId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tithe creation failed: Member {MemberId} not found"
    )]
    public static partial void MemberNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tithe error: unexpected failure - {Message}")]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
