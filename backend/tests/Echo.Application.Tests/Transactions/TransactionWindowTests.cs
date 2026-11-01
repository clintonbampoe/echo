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
        var transaction = TestData.Shared.CreateTransaction();
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
    public void Paginate_Cursor_KeepsRowsAfterItNewestDateFirstThenById()
    {
        Transaction Row(DateOnly date, string id)
        {
            var transaction = On(date);
            transaction.Id = Guid.Parse(id);
            return transaction;
        }

        var cursor = new TransactionCursor
        {
            TransactionDate = new DateOnly(2026, 3, 1),
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
        };
        var sameDateBefore = Row(new DateOnly(2026, 3, 1), "00000000-0000-0000-0000-000000000004");
        var sameDateAfter = Row(new DateOnly(2026, 3, 1), "00000000-0000-0000-0000-000000000006");
        var older = Row(new DateOnly(2026, 2, 1), "00000000-0000-0000-0000-000000000001");
        var newer = Row(new DateOnly(2026, 4, 1), "00000000-0000-0000-0000-000000000001");

        var res = new[] { sameDateBefore, sameDateAfter, older, newer }
            .AsQueryable()
            .Paginate(cursor, 10)
            .ToList();

        Assert.Equal([sameDateAfter, older], res);
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

    [Fact]
    public void Filter_ByPaymentMethod_KeepsOnlyThatMethod()
    {
        var cash = On(new DateOnly(2026, 3, 1));
        cash.PaymentMethod = PaymentMethod.Cash;
        var momo = On(new DateOnly(2026, 3, 1));
        momo.PaymentMethod = PaymentMethod.MobileMoney;
        var unrecorded = On(new DateOnly(2026, 3, 1));
        unrecorded.PaymentMethod = null;

        var res = new[] { cash, momo, unrecorded }
            .AsQueryable()
            .Filter(new TransactionFilters { PaymentMethod = PaymentMethod.Cash, From = null, To = null })
            .ToList();

        Assert.Equal([cash], res);
    }

    [Fact]
    public void TotalsByType_AddsUpIncomeAndExpensesSeparately()
    {
        Transaction Row(TransactionType type, decimal amount)
        {
            var transaction = On(new DateOnly(2026, 3, 1));
            transaction.TransactionType = type;
            transaction.Amount = amount;
            return transaction;
        }

        var rows = new[]
        {
            Row(TransactionType.Income, 100m),
            Row(TransactionType.Income, 50m),
            Row(TransactionType.Expense, 30m),
        };

        var res = rows.AsQueryable().TotalsByType().ToDictionary(t => t.Type, t => t.Total);

        Assert.Equal(new Dictionary<TransactionType, decimal>
        {
            [TransactionType.Income] = 150m,
            [TransactionType.Expense] = 30m,
        }, res);
    }
}
