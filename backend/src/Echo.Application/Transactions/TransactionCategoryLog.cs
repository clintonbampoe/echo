using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public static partial class TransactionCategoryLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category list: {Count} categories returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Transaction category not found: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Create ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category created: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int categoryId);

    // --- Update ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category updated: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction category update failed: {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Delete ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category deleted: {CategoryId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction category delete failed: {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, int categoryId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Transaction category error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
