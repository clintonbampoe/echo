using Echo.Application.Events;
using Echo.Domain.Events;
using Echo.Shared.Pagination;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventListOrderingTests
{
    private static readonly Guid Id1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Id2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid Id3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private static Event On(DateOnly start, Guid id)
    {
        var e = EventFactory.NewEntity();
        e.Id = id;
        e.StartDate = start;
        return e;
    }

    // Two events on the same day, then an earlier one.
    private static readonly Event A = On(new DateOnly(2026, 10, 10), Id1);
    private static readonly Event B = On(new DateOnly(2026, 10, 10), Id2);
    private static readonly Event C = On(new DateOnly(2026, 9, 1), Id3);

    [Fact]
    public void OrderForList_Ascending_IsOldestFirst_TiesByIdAscending()
    {
        var res = new[] { B, C, A }.AsQueryable().OrderForList(SortDirection.Asc).ToList();

        Assert.Equal([C, A, B], res);
    }

    [Fact]
    public void OrderForList_Descending_IsNewestFirst_TiesByIdDescending()
    {
        var res = new[] { C, A, B }.AsQueryable().OrderForList(SortDirection.Desc).ToList();

        Assert.Equal([B, A, C], res);
    }

    [Fact]
    public void Paginate_Ascending_ContinuesAfterTheCursor()
    {
        var ordered = new[] { A, B, C }.AsQueryable().OrderForList(SortDirection.Asc);
        var cursor = new EventCursor { StartDate = C.StartDate, Id = C.Id, Order = SortDirection.Asc };

        Assert.Equal([A, B], ordered.Paginate(cursor, 10).ToList());
    }

    [Fact]
    public void Paginate_Descending_ContinuesAfterTheCursor_WithoutSkippingOrRepeatingTiedDays()
    {
        var ordered = new[] { A, B, C }.AsQueryable().OrderForList(SortDirection.Desc);

        var firstPage = ordered.Paginate(null, 2).ToList();
        var last = firstPage[^1];
        var cursor = new EventCursor { StartDate = last.StartDate, Id = last.Id, Order = SortDirection.Desc };
        var secondPage = ordered.Paginate(cursor, 2).ToList();

        Assert.Equal([B, A], firstPage);
        Assert.Equal([C], secondPage);
    }

    [Fact]
    public void UsableCursor_IsKept_WhenItMatchesTheRequestedDirection()
    {
        var cursor = new EventCursor { StartDate = C.StartDate, Id = C.Id, Order = SortDirection.Desc };

        Assert.Same(cursor, EventService.UsableCursor(cursor, SortDirection.Desc));
    }

    [Fact]
    public void UsableCursor_IsDropped_WhenTheDirectionChanged_SoThePageStartsAgain()
    {
        var cursor = new EventCursor { StartDate = C.StartDate, Id = C.Id, Order = SortDirection.Asc };

        Assert.Null(EventService.UsableCursor(cursor, SortDirection.Desc));
        Assert.Null(EventService.UsableCursor(null, SortDirection.Desc));
    }
}
