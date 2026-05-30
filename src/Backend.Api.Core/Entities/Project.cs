using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Project : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ProjectManagerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; } = string.Empty;
}
