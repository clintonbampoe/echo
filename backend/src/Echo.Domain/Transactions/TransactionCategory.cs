using Echo.Domain.Congregations;

namespace Echo.Domain.Transactions;

public class TransactionCategory : ISearchable, ISoftDeletable
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public TransactionType CategoryType { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
