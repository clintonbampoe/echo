using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Project : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public Guid CategoryId { get; init; }
    public ProjectCategory Category { get; init; } = null!;
    public Guid ManagerId { get; init; }
    public Member Manager { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public ProjectStatus Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
