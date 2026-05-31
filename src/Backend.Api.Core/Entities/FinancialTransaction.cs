using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class FinancialTransaction : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public DateTime CreatedAt { get; init; }
    public TransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
