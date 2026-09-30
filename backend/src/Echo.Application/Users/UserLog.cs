using Microsoft.Extensions.Logging;

namespace Echo.Application.Users;

public static partial class UserLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User created: {UserId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User updated: {UserId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User deleted: {UserId} soft-deleted")]
    public static partial void Deleted(ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User not found: {UserId} requested")]
    public static partial void NotFound(ILogger logger, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User creation failed: Email {EmailAddress} is already taken"
    )]
    public static partial void EmailTaken(ILogger logger, string emailAddress);

    [LoggerMessage(Level = LogLevel.Error, Message = "User error: unexpected failure - {Message}")]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
