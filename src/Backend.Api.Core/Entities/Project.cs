using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Project
{
    public int ProjectId { get; init; }
    public Guid UniqueId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int ProjectManagerId { get; init; }
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; } = string.Empty;
}
