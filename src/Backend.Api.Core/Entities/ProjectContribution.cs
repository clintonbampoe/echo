using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class ProjectContribution
{
    public int ContributionId { get; init; }
    public Guid UniqueId { get; init; }
    public int ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly ContributedDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string Description { get; init; } = string.Empty;
}
