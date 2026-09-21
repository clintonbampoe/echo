using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Transactions;

namespace Echo.Domain.Tithes;

public class Tithe : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public decimal Amount { get; set; }
    public int ForYear { get; set; }
    public MonthOfYear ForMonth { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateOnly CollectionDate { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
