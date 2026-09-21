using Echo.Domain.Congregations;
using Echo.Domain.Transactions;

namespace Echo.Domain.Projects;

public class ProjectContribution : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly DateContributed { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
