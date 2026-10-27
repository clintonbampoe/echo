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
/// Default window when From/To are omitted: this week to
/// today (by CollectionDate). No default window when
/// MemberId, Year or Month is set.
/// </summary>
public record MemberTitheFilters
{
    public PaymentMethod? PaymentMethod { get; init; }
    public int? Year { get; init; }
    public MonthOfYear? Month { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

/// <summary>The filters of the whole congregation's tithes: those of one member's, and which members.</summary>
public record TitheFilters : MemberTitheFilters
{
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; }
}

public record TitheSummaryDto
{
    public decimal TotalCollected { get; init; }
    public int UniqueTithers { get; init; }
    public string? MostUsedPaymentMethod { get; init; }
    public decimal AveragePerMember { get; init; }

    /// <summary>The biggest single tithe in the window. Zero when there are none.</summary>
    public decimal LargestTithe { get; init; }

    /// <summary>How the tithes were paid, the method with the most tithes first.</summary>
    public IReadOnlyList<TitheMethodDto> ByMethod { get; init; } = [];
    public DateWindow? Window { get; init; }
}

/// <summary>
/// Filters for the monthly summary. Months and To choose the months to chart; the other filters
/// choose the tithes. Tithes are grouped by the month they are for, not by collection date.
/// </summary>
public record TitheMonthlyFilters
{
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int Months { get; init; } = MonthRange.DefaultMonths;

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

/// <summary>How many tithes were paid one way, and how much they came to.</summary>
public record TitheMethodDto
{
    public required string Method { get; init; }
    public int Count { get; init; }
    public decimal Total { get; init; }
}

public record TitheMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalCollected { get; init; }
    public int UniqueTithers { get; init; }

    /// <summary>What a tither gave on average this month: the total over the people who gave.</summary>
    public decimal AveragePerMember { get; init; }
}
