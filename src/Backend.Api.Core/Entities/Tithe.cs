using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Tithe
{
    public int TitheId { get; }
    public Guid UniqueId { get; }
    public int MemberId { get; }
    public int Decimal { get; }
    public PaymentMethod PaymentMethod { get; }
    public DateOnly CollectionDate { get; }
    public MonthOfYear ForMonthOfYear { get; }
    public int ForYear { get; }
    public string Description { get; }
}
