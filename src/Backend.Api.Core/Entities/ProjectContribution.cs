using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class ProjectContribution : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Description { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
