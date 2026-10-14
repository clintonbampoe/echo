using Echo.Application.Events;
using Echo.Domain.Events;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Event StartingOn(DateOnly date)
    {
        var entity = EventFactory.NewEntity();
        entity.StartDate = date;
        return entity;
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = EventService.ResolveWindow(new EventFilters(), _windows);

        Assert.Equal(new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true), w);
    }

    [Fact]
    public void ResolveWindow_OrganizerIdSet_ReturnsNull()
    {
        var filters = new EventFilters { OrganizerId = Guid.NewGuid() };

        Assert.Null(EventService.ResolveWindow(filters, _windows));
    }

    [Fact]
    public void ResolveWindow_OrganizerIdSetWithDates_UsesTheDates()
    {
        var filters = new EventFilters
        {
            OrganizerId = Guid.NewGuid(),
            From = new DateOnly(2000, 1, 1),
            To = new DateOnly(2100, 12, 31),
        };

        Assert.Equal(
            new DateWindow(new DateOnly(2000, 1, 1), new DateOnly(2100, 12, 31), false),
            EventService.ResolveWindow(filters, _windows)
        );
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            StartingOn(new DateOnly(2025, 12, 31)),
            StartingOn(new DateOnly(2026, 1, 1)),
            StartingOn(new DateOnly(2026, 12, 31)),
            StartingOn(new DateOnly(2027, 1, 1)),
        };
        var filters = new EventFilters
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }
}
