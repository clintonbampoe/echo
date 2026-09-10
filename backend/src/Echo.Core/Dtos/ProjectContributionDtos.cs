using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record ProjectContributionCreateDto
{
    public Guid ProjectId { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }

    [StringLength(2000)]
    public required string? Description { get; init; }
}

public record ProjectContributionUpdateDto
{
    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; init; }

    public DateOnly? DateContributed { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record ProjectContributionListResponseDto
{
    public Guid Id { get; init; }
    public required string ProjectName { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
}

public record ProjectContributionResponseDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public required string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProjectContributionSummaryDto
{
    public required decimal TotalRaised { get; init; }
    public required decimal TargetGoal { get; init; }
    public required int Contributors { get; init; }
    public required DateOnly? MostRecentEntryDate { get; init; }
}
