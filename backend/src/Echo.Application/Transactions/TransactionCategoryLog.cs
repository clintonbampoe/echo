using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public static partial class TransactionCategoryLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category created: {TransactionCategoryId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, int transactionCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category updated: {TransactionCategoryId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, int transactionCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction category deleted: {TransactionCategoryId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, int transactionCategoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction category not found: {TransactionCategoryId} requested"
    )]
    public static partial void NotFound(ILogger logger, int transactionCategoryId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Transaction category error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
