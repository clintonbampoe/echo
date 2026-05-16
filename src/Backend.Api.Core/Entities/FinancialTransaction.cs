using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class FinancialTransaction
{
    public int TransactionId { get; init; }
    public Guid UniqueId { get; init; }
    public int CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
}
