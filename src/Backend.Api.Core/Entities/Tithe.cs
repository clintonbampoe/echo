using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Tithe : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public int Decimal { get; set; }
    public int ForYear { get; set; }
    public MonthOfYear ForMonth { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateOnly CollectionDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
