using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Transaction : ICongregationEntity, ISoftDeletableEntity
{
    public Guid CategoryId { get; set; }
    public TransactionCategory Category { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
