using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class ProjectContribution
{
    public int ContributionId { get; }
    public Guid UniqueId { get; }
    public int ProjectId { get; }
    public decimal Amount { get; }
    public DateOnly ContributedDate { get; }
    public PaymentMethod PaymentMethod { get; }
    public string Description { get; }
}
