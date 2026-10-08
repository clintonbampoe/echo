using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public static partial class TransactionLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction list: {Count} transactions returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction found: {TransactionId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction not found: {TransactionId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid transactionId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction create: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction create failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction update: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction update failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction created: {TransactionId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction updated: {TransactionId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction update failed: {TransactionId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid transactionId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction deleted: {TransactionId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction delete failed: {TransactionId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid transactionId
    );

    // --- Summary ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction summary: income {TotalIncome}, expenses {TotalExpenses} for congregation {CongregationId}"
    )]
    public static partial void Summarized(
        ILogger logger,
        Guid congregationId,
        decimal totalIncome,
        decimal totalExpenses
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Transaction error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
