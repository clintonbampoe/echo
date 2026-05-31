using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class FinancialTransaction : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
