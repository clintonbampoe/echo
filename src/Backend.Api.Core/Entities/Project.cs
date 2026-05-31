using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Project : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ProjectManagerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public ProjectStatus Status { get; set  ; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
