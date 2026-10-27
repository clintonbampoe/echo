using Echo.Domain.Congregations;

namespace Echo.Domain.Transactions;

public class Transaction : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public int CategoryId { get; set; }
    public TransactionCategory Category { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }

    /// <summary>How it was paid. Empty for transactions recorded before it was asked.</summary>
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
