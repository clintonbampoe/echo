using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Tithe : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int Decimal { get; set; }
    public int ForYear { get; set; }
    public MonthOfYear ForMonth { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateOnly CollectionDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
