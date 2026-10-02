using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public static partial class AssetCategoryLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category list: {Count} categories returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category found: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, int categoryId);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category search: {Count} results for query '{Query}' in congregation {CongregationId}"
    )]
    public static partial void Searched(
        ILogger logger,
        Guid congregationId,
        string query,
        int count
    );

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset category not found: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Create ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category created: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int categoryId);

    // --- Update ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category updated: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset category update failed: {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Delete ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Asset category deleted: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Asset category delete failed: {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Asset category error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
