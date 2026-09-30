using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public static partial class TransactionLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction created: {TransactionId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction updated: {TransactionId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Transaction deleted: {TransactionId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction not found: {TransactionId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid transactionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Transaction creation failed: Category {CategoryId} not found"
    )]
    public static partial void CategoryNotFound(ILogger logger, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Transaction error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
