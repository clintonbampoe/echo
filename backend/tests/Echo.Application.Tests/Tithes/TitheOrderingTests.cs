using Echo.Application.Tithes;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests.Tithes;

[Trait("Category", "Unit")]
public class TitheOrderingTests
{
    private static Tithe NewTithe(DateOnly collectionDate, Guid id)
    {
        var tithe = TestData.Shared.CreateTithe();
        tithe.CollectionDate = collectionDate;
        tithe.Id = id;
        return tithe;
    }

    [Fact]
    public void OrderForList_SortsByCollectionDateDescendingThenIdAscending()
    {
        var oldest = NewTithe(
            new DateOnly(2026, 1, 4),
            Guid.Parse("00000000-0000-0000-0000-000000000001")
        );
        var newerHighId = NewTithe(
            new DateOnly(2026, 3, 1),
            Guid.Parse("00000000-0000-0000-0000-000000000009")
        );
        var newerLowId = NewTithe(
            new DateOnly(2026, 3, 1),
            Guid.Parse("00000000-0000-0000-0000-000000000002")
        );

        var res = new[] { oldest, newerHighId, newerLowId }.AsQueryable().OrderForList().ToList();

        Assert.Equal([newerLowId.Id, newerHighId.Id, oldest.Id], res.Select(t => t.Id));
    }

    [Fact]
    public void Paginate_Cursor_KeepsRowsAfterItInListOrder()
    {
        var cursorDate = new DateOnly(2026, 3, 1);
        var cursorId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        var sameDateAfter = NewTithe(cursorDate, Guid.Parse("00000000-0000-0000-0000-000000000006"));
        var sameDateBefore = NewTithe(cursorDate, Guid.Parse("00000000-0000-0000-0000-000000000004"));
        var older = NewTithe(new DateOnly(2026, 2, 1), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var newer = NewTithe(new DateOnly(2026, 4, 1), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var cursor = new TitheCursor { CollectionDate = cursorDate, Id = cursorId };

        var res = new[] { sameDateBefore, sameDateAfter, older, newer }
            .AsQueryable()
            .Paginate(cursor, 10)
            .ToList();

        Assert.Equal([sameDateAfter, older], res);
    }

    [Fact]
    public void ByMethod_TotalsAndCountsTheTithesOfEachPaymentMethod()
    {
        Tithe Paid(PaymentMethod method, decimal amount)
        {
            var tithe = TestData.Shared.CreateTithe();
            tithe.PaymentMethod = method;
            tithe.Amount = amount;
            return tithe;
        }

        var rows = new[]
        {
            Paid(PaymentMethod.Cash, 50m),
            Paid(PaymentMethod.Cash, 70m),
            Paid(PaymentMethod.MobileMoney, 200m),
        };

        var res = rows.AsQueryable().ByMethod().OrderBy(m => m.Method).ToList();

        Assert.Equal(
            [new MethodShare(PaymentMethod.Cash, 2, 120m), new MethodShare(PaymentMethod.MobileMoney, 1, 200m)],
            res
        );
    }

    [Fact]
    public void MostUsed_IsTheMethodWithTheMostTithes_AndATieGoesToTheMethodThatSortsFirst()
    {
        var shares = new[]
        {
            new MethodShare(PaymentMethod.MobileMoney, 3, 10m),
            new MethodShare(PaymentMethod.Cash, 3, 10m),
            new MethodShare(PaymentMethod.Cheque, 1, 10m),
        };

        Assert.Equal(PaymentMethod.Cash, TitheStats.MostUsed(shares));
        Assert.Null(TitheStats.MostUsed([]));
    }
}
