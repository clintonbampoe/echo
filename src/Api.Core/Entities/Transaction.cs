using Api.Core.Entities.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities;

public class Transaction : IPrimaryEntity
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int CategoryId { get; set; }
    public TransactionCategory Category { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
