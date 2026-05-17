using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Tithe
{
    public int TitheId { get; init; }
    public Guid UniqueId { get; init; }
    public int MemberId { get; init; }
    public int Decimal { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public MonthOfYear ForMonthOfYear { get; init; }
    public int ForYear { get; init; }
    public string? Description { get; init; } = string.Empty;
}
