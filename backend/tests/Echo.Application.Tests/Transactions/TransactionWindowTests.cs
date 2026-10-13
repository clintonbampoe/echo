using Echo.Application.Transactions;
using Echo.Domain.Transactions;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Transactions;

[Trait("Category", "Unit")]
public class TransactionWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Transaction On(DateOnly date)
    {
        var transaction = TransactionFactory.NewEntity();
        transaction.TransactionDate = date;
        return transaction;
    }

    [Fact]
    public void Filter_NoDates_AppliesNoDateRestriction()
    {
        var rows = new[] { On(new DateOnly(2020, 1, 1)), On(new DateOnly(2026, 10, 14)) };

        var res = rows.AsQueryable().Filter(new TransactionFilters()).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            On(new DateOnly(2026, 10, 10)),
            On(new DateOnly(2026, 10, 11)),
            On(new DateOnly(2026, 10, 14)),
            On(new DateOnly(2026, 10, 15)),
        };
        var filters = new TransactionFilters
        {
            From = new DateOnly(2026, 10, 11),
            To = new DateOnly(2026, 10, 14),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void ResolveWindow_Default_IsThisWeekToDate()
    {
        var w = TransactionService.ResolveWindow(new TransactionFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 14), true),
            w
        );
    }
}
