using Microsoft.Extensions.Logging;

namespace Echo.Application.Users;

public static partial class UserLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User list: {Count} users returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "User found: {UserId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User not found: {UserId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid userId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User create failed: email already taken in congregation {CongregationId}"
    )]
    public static partial void CreateEmailTaken(ILogger logger, Guid congregationId);

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User update failed: email already taken in congregation {CongregationId}"
    )]
    public static partial void UpdateEmailTaken(ILogger logger, Guid congregationId);

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User created: {UserId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User updated: {UserId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User update failed: {UserId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User deleted: {UserId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User delete failed: {UserId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid userId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "User error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
