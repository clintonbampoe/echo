using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Project : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public Guid CategoryId { get; set; }
    public ProjectCategory Category { get; set; } = null!;
    public Guid ManagerId { get; set; }
    public Member Manager { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public ProjectStatus Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
