using Api.Core.Entities.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities;

public class ProjectContribution : IPrimaryEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly DateContributed { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; }
}
