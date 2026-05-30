using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Tithe : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public int Decimal { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; } = string.Empty;
}
