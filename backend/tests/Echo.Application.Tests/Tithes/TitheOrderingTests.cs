using Echo.Application.Tithes;
using Echo.Domain.Tithes;

namespace Echo.Application.Tests.Tithes;

[Trait("Category", "Unit")]
public class TitheOrderingTests
{
    private static Tithe NewTithe(DateOnly collectionDate, Guid id)
    {
        var tithe = TitheFactory.NewEntity();
        tithe.CollectionDate = collectionDate;
        tithe.Id = id;
        return tithe;
    }

    [Fact]
    public void OrderForList_SortsByCollectionDateDescendingThenIdAscending()
    {
        var oldest = NewTithe(new DateOnly(2026, 1, 4), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var newerHighId = NewTithe(new DateOnly(2026, 3, 1), Guid.Parse("00000000-0000-0000-0000-000000000009"));
        var newerLowId = NewTithe(new DateOnly(2026, 3, 1), Guid.Parse("00000000-0000-0000-0000-000000000002"));

        var res = new[] { oldest, newerHighId, newerLowId }.AsQueryable().OrderForList().ToList();

        Assert.Equal([newerLowId.Id, newerHighId.Id, oldest.Id], res.Select(t => t.Id));
    }
}
