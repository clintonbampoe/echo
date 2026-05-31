using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class ProjectContribution : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid ProjectId { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal Amount { get; set; }
    public DateOnly DateContributed { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
