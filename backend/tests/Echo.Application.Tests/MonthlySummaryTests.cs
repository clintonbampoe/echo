using Echo.Application.Tests.Tithes;
using Echo.Application.Tests.Transactions;
using Echo.Application.Tithes;
using Echo.Application.Transactions;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Shared.Utilities;

namespace Echo.Application.Tests;

[Trait("Category", "Unit")]
public class MonthlySummaryTests
{
    private static Transaction Txn(
        DateOnly date,
        TransactionType type,
        decimal amount,
        string category = "General"
    )
    {
        var t = TransactionFactory.NewEntity();
        t.Id = Guid.NewGuid();
        t.TransactionDate = date;
        t.TransactionType = type;
        t.Amount = amount;
        t.Category.Name = category;
        t.CategoryId = category.GetHashCode();
        return t;
    }

    private static Tithe NewTithe(
        int forYear,
        MonthOfYear forMonth,
        DateOnly collected,
        Guid memberId,
        decimal amount
    )
    {
        var t = TitheFactory.NewEntity();
        t.Id = Guid.NewGuid();
        t.ForYear = forYear;
        t.ForMonth = forMonth;
        t.CollectionDate = collected;
        t.MemberId = memberId;
        t.Amount = amount;
        return t;
    }

    [Fact]
    public void Transactions_MonthlyTotals_GroupsByMonthAndSplitsIncomeFromExpenses()
    {
        var rows = new[]
        {
            Txn(new DateOnly(2026, 9, 3), TransactionType.Income, 100m),
            Txn(new DateOnly(2026, 9, 20), TransactionType.Income, 50m),
            Txn(new DateOnly(2026, 9, 21), TransactionType.Expense, 30m),
            Txn(new DateOnly(2026, 10, 2), TransactionType.Expense, 70m),
        };

        var res = rows.AsQueryable().MonthlyTotals().ToList().OrderBy(m => m.Month).ToList();

        Assert.Equal(2, res.Count);
        Assert.Equal(
            (2026, 9, 150m, 30m, 120m),
            (res[0].Year, res[0].Month, res[0].TotalIncome, res[0].TotalExpenses, res[0].Net)
        );
        Assert.Equal(
            (2026, 10, 0m, 70m, -70m),
            (res[1].Year, res[1].Month, res[1].TotalIncome, res[1].TotalExpenses, res[1].Net)
        );
    }

    [Fact]
    public void Transactions_WholeMonths_KeepsTransactionsEarlierInTheFirstMonthAndLaterInTheLast()
    {
        var months = MonthRange.Between(new DateOnly(2026, 1, 15), new DateOnly(2026, 3, 10));
        var requested = new TransactionFilters
        {
            From = new DateOnly(2026, 1, 15),
            To = new DateOnly(2026, 3, 10),
        };
        var rows = new[]
        {
            Txn(new DateOnly(2025, 12, 31), TransactionType.Income, 1m),
            Txn(new DateOnly(2026, 1, 5), TransactionType.Income, 1m),
            Txn(new DateOnly(2026, 3, 31), TransactionType.Income, 1m),
            Txn(new DateOnly(2026, 4, 1), TransactionType.Income, 1m),
        };

        var filters = TransactionService.WholeMonths(requested, months);
        var kept = rows.AsQueryable()
            .Filter(filters)
            .Select(t => t.TransactionDate)
            .OrderBy(d => d)
            .ToList();

        Assert.Equal([new DateOnly(2026, 1, 5), new DateOnly(2026, 3, 31)], kept);
    }

    [Fact]
    public void Transactions_FillMonths_ReturnsEveryMonthWithZerosWhereThereIsNothing()
    {
        var months = MonthRange.Between(new DateOnly(2026, 8, 1), new DateOnly(2026, 10, 31));
        var rows = new List<TransactionMonthlyDto>
        {
            new()
            {
                Year = 2026,
                Month = 9,
                TotalIncome = 150m,
                TotalExpenses = 30m,
            },
        };

        var res = TransactionService.FillMonths(months, rows);

        Assert.Equal([8, 9, 10], res.Select(m => m.Month));
        Assert.Equal((0m, 0m), (res[0].TotalIncome, res[0].TotalExpenses));
        Assert.Equal(150m, res[1].TotalIncome);
        Assert.Equal((0m, 0m), (res[2].TotalIncome, res[2].TotalExpenses));
    }

    [Fact]
    public void Transactions_TotalsByCategory_SplitsByTypeAndSortsByTotalDescending()
    {
        var rows = new[]
        {
            Txn(new DateOnly(2026, 10, 1), TransactionType.Expense, 40m, "Utilities"),
            Txn(new DateOnly(2026, 10, 2), TransactionType.Expense, 60m, "Utilities"),
            Txn(new DateOnly(2026, 10, 3), TransactionType.Expense, 500m, "Building"),
            Txn(new DateOnly(2026, 10, 4), TransactionType.Income, 200m, "Offerings"),
        };

        var res = rows.AsQueryable().TotalsByCategory().ToList();

        Assert.Equal(["Building", "Offerings", "Utilities"], res.Select(c => c.CategoryName));
        var utilities = res.Single(c => c.CategoryName == "Utilities");
        Assert.Equal(
            (100m, 2, TransactionType.Expense),
            (utilities.Total, utilities.Count, utilities.Type)
        );
    }

    [Fact]
    public void Tithes_InYears_KeepsOnlyTithesForThoseYears()
    {
        var member = Guid.NewGuid();
        var rows = new[]
        {
            NewTithe(2025, MonthOfYear.December, new DateOnly(2025, 12, 5), member, 10m),
            NewTithe(2026, MonthOfYear.October, new DateOnly(2026, 10, 5), member, 10m),
            NewTithe(2027, MonthOfYear.January, new DateOnly(2027, 1, 5), member, 10m),
        };

        var res = rows.AsQueryable()
            .InYears(new YearMonth(2026, 1), new YearMonth(2026, 12))
            .ToList();

        Assert.Single(res);
        Assert.Equal(MonthOfYear.October, res[0].ForMonth);
    }

    [Fact]
    public void Tithes_MonthlyTotals_GroupsByTheMonthTitheIsForNotTheCollectionDate()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var rows = new[]
        {
            // December 2026 tithe, given in January 2027: belongs to December.
            NewTithe(2026, MonthOfYear.December, new DateOnly(2027, 1, 3), a, 100m),
            NewTithe(2026, MonthOfYear.December, new DateOnly(2026, 12, 20), a, 50m),
            NewTithe(2026, MonthOfYear.December, new DateOnly(2026, 12, 21), b, 25m),
            NewTithe(2027, MonthOfYear.January, new DateOnly(2027, 1, 10), b, 10m),
        };

        var res = rows.AsQueryable()
            .MonthlyTotals()
            .ToList()
            .Select(r => r.ToDto())
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        Assert.Equal(2, res.Count);
        Assert.Equal(
            (2026, 12, 175m, 2),
            (res[0].Year, res[0].Month, res[0].TotalCollected, res[0].UniqueTithers)
        );
        Assert.Equal(
            (2027, 1, 10m, 1),
            (res[1].Year, res[1].Month, res[1].TotalCollected, res[1].UniqueTithers)
        );
    }

    [Fact]
    public void Tithes_FillMonths_ReturnsEveryMonthWithZerosWhereThereIsNothing()
    {
        var months = MonthRange.Between(new DateOnly(2026, 11, 1), new DateOnly(2027, 1, 31));
        var rows = new List<TitheMonthlyDto>
        {
            new()
            {
                Year = 2026,
                Month = 12,
                TotalCollected = 175m,
                UniqueTithers = 2,
            },
        };

        var res = TitheService.FillMonths(months, rows);

        Assert.Equal([(2026, 11), (2026, 12), (2027, 1)], res.Select(m => (m.Year, m.Month)));
        Assert.Equal((0m, 0), (res[0].TotalCollected, res[0].UniqueTithers));
        Assert.Equal(175m, res[1].TotalCollected);
        Assert.Equal((0m, 0), (res[2].TotalCollected, res[2].UniqueTithers));
    }
}
