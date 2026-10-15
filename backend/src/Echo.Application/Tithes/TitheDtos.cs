using System.ComponentModel.DataAnnotations;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Shared.Utilities;

namespace Echo.Application.Tithes;

public record TitheCreateDto
{
    public Guid MemberId { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [Range(1900, 2100)]
    public int ForYear { get; init; }

    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

// The member a tithe belongs to is fixed when it is recorded, so an update has no member. A tithe recorded
// against the wrong member is deleted and recorded again.
public record TitheUpdateDto
{
    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; init; }

    [Range(1900, 2100)]
    public int? ForYear { get; init; }

    public MonthOfYear? ForMonth { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateOnly? CollectionDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TitheResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public decimal Amount { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TitheCursor
{
    public DateOnly CollectionDate { get; init; }
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this week to today (by CollectionDate).
/// No default window when MemberId, Year or Month is set.
/// </summary>
public record TitheFilters
{
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; } // partial, case-insensitive match on the member's name
    public PaymentMethod? PaymentMethod { get; init; }
    public int? Year { get; init; }
    public MonthOfYear? Month { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on CollectionDate
}

public record TitheSummaryDto
{
    public decimal TotalCollected { get; init; }
    public int UniqueTithers { get; init; }
    public string? MostUsedPaymentMethod { get; init; }
    public decimal AveragePerMember { get; init; }
    public DateWindow? Window { get; init; }
}

/// <summary>
/// Filters for the monthly summary. From and To choose the months to chart (any day in the first
/// and last month); tithes are grouped by the month they are for, not by collection date.
/// </summary>
public record TitheMonthlyFilters
{
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; } // partial, case-insensitive match on the member's name
    public PaymentMethod? PaymentMethod { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record TitheMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalCollected { get; init; }
    public int UniqueTithers { get; init; }
}
