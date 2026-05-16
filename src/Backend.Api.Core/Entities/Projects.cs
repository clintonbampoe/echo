using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Projects
{
    public int ProjectId { get; }
    public Guid UniqueId { get; }
    public string Title { get; }
    public int CategoryId { get; }
    public int ProjectManagerId { get; }
    public decimal TargetAmount { get; }
    public ProjectStatus Status { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public string Description { get; }
}
