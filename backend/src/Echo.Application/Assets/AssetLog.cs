using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public static partial class AssetLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset creation: initiating for congregation {CongregationId}"
    )]
    public static partial void Creating(ILogger logger, Guid congregationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset created: {AssetId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset updated: {AssetId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid assetId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Asset deleted: {AssetId} soft-deleted")]
    public static partial void Deleted(ILogger logger, Guid assetId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Asset not found: {AssetId} requested")]
    public static partial void NotFound(ILogger logger, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset creation failed: Category {CategoryId} not found"
    )]
    public static partial void CategoryNotFound(ILogger logger, int categoryId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Asset error: unexpected failure - {Message}")]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
