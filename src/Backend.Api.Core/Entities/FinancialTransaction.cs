using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class FinancialTransaction : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; } = string.Empty;
}
