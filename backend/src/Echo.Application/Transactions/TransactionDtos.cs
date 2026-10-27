using System.ComponentModel.DataAnnotations;
using Echo.Domain.Transactions;
using Echo.Shared.Utilities;

namespace Echo.Application.Transactions;

public record TransactionCreateDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    public PaymentMethod? PaymentMethod { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TransactionUpdateDto
{
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    public TransactionType? TransactionType { get; init; }
    public DateOnly? TransactionDate { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; init; }

    public PaymentMethod? PaymentMethod { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TransactionResponseDto
{
    public Guid Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }

    /// <summary>How it was paid. Empty for transactions recorded before it was asked.</summary>
    public PaymentMethod? PaymentMethod { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TransactionCursor
{
    public DateOnly TransactionDate { get; init; }
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this week to today (by TransactionDate).
/// </summary>
public record TransactionFilters
{
    public TransactionType? TransactionType { get; init; }
    public int? CategoryId { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }

    /// <summary>Text to look for in the description or the category name (case-insensitive).</summary>
    public string? Search { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on TransactionDate
}

public record TransactionSummaryDto
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal Net { get; init; }

    /// <summary>Income in the window before this one: as many days, ending the day before `from`.</summary>
    public decimal PreviousTotalIncome { get; init; }

    /// <summary>Expenses in the window before this one.</summary>
    public decimal PreviousTotalExpenses { get; init; }
    public string? MostActiveCategory { get; init; }
    public List<TransactionCategoryTotalDto> ByCategory { get; init; } = [];
    public DateWindow? Window { get; init; }
}

/// <summary>
/// Filters for the monthly summary. Months and To choose the months to chart; the other filters
/// choose the transactions.
/// </summary>
public record TransactionMonthlyFilters
{
    public int? CategoryId { get; init; }
    public TransactionType? TransactionType { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int Months { get; init; } = MonthRange.DefaultMonths;

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

public record TransactionMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal Net => TotalIncome - TotalExpenses;
}

public record TransactionCategoryTotalDto
{
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public TransactionType Type { get; init; }
    public decimal Total { get; init; }
    public int Count { get; init; }
}
