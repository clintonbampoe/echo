using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class ProjectContribution : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public Guid ProjectId { get; init; }
    public Project Project { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public DateOnly DateContributed { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
