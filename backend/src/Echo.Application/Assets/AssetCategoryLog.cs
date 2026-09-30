using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public static partial class AssetCategoryLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category created: {AssetCategoryId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, int assetCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category updated: {AssetCategoryId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, int assetCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category deleted: {AssetCategoryId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, int assetCategoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset category not found: {AssetCategoryId} requested"
    )]
    public static partial void NotFound(ILogger logger, int assetCategoryId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Asset category error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
