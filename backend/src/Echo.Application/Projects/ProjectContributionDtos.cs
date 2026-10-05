using System.ComponentModel.DataAnnotations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;

namespace Echo.Application.Projects;

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

public record ProjectContributionCursor
{
    public DateOnly DateContributed { get; init; }
    public Guid Id { get; init; }
}

public record ProjectContributionFilters
{
    public Guid? ProjectId { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record ProjectContributionSummaryDto
{
    public decimal TotalContributed { get; init; }
    public int TotalContributions { get; init; }
    public decimal AverageAmount { get; init; }
    public string? MostUsedPaymentMethod { get; init; }
}
