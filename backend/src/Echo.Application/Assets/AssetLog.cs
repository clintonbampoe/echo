using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public static partial class AssetLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset list: {Count} assets returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Asset found: {AssetId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset not found: {AssetId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid assetId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset create: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset create failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset update: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset update failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset created: {AssetId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset updated: {AssetId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset update failed: {AssetId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset deleted: {AssetId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid assetId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset delete failed: {AssetId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid assetId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Asset error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
