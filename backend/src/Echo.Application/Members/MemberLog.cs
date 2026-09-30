using Microsoft.Extensions.Logging;

namespace Echo.Application.Members;

public static partial class MemberLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member created: {MemberId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member updated: {MemberId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Member deleted: {MemberId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid memberId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Member not found: {MemberId} requested")]
    public static partial void NotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Member error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
