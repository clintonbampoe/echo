using Echo.Application.Events;
using Echo.Domain.Events;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventSeriesTests
{
    private static Event On(DateOnly start, string id, int? capacity = null)
    {
        var e = TestData.Shared.CreateEvent();
        e.Id = Guid.Parse(id);
        e.StartDate = start;
        e.Capacity = capacity;
        return e;
    }

    private static EventRegistration Registered(Event e, bool deleted = false)
    {
        var r = TestData.Shared.CreateEventRegistration();
        r.Id = Guid.NewGuid();
        r.EventId = e.Id;
        r.DeletedAt = deleted ? DateTime.UtcNow : null;
        return r;
    }

    private static EventAttendance CheckedIn(Event e, bool deleted = false)
    {
        var a = TestData.Shared.CreateEventAttendance();
        a.Id = Guid.NewGuid();
        a.EventId = e.Id;
        a.DeletedAt = deleted ? DateTime.UtcNow : null;
        return a;
    }

    private static readonly Event Jan = On(new DateOnly(2026, 1, 10), "00000000-0000-0000-0000-000000000001", 100);
    private static readonly Event Jan2 = On(new DateOnly(2026, 1, 24), "00000000-0000-0000-0000-000000000002");
    private static readonly Event Mar = On(new DateOnly(2026, 3, 7), "00000000-0000-0000-0000-000000000003", 50);

    [Fact]
    public void Series_ListsTheNewestEventsFirst_UpToTheLimit_WithTheirCounts()
    {
        var regs = new[] { Registered(Jan), Registered(Mar), Registered(Mar), Registered(Mar, deleted: true) };
        var checkIns = new[] { CheckedIn(Mar), CheckedIn(Jan2) };

        var res = new[] { Jan, Jan2, Mar }
            .AsQueryable()
            .Series(regs.Where(r => r.DeletedAt == null).AsQueryable(), checkIns.AsQueryable(), limit: 2)
            .ToList();

        Assert.Equal([Mar.Id, Jan2.Id], res.Select(s => s.EventId));
        Assert.Equal((2, 1, 50), (res[0].RegistrationCount, res[0].AttendanceCount, res[0].Capacity));
        Assert.Equal((0, 1, (int?)null), (res[1].RegistrationCount, res[1].AttendanceCount, res[1].Capacity));
    }

    [Fact]
    public void MonthlyTotals_CountsEventsAndTheirRegistrationsAndCheckInsByTheMonthTheyStart()
    {
        var regs = new[] { Registered(Jan), Registered(Jan), Registered(Jan2), Registered(Mar) };
        var checkIns = new[] { CheckedIn(Jan), CheckedIn(Mar), CheckedIn(Mar) };

        var res = new[] { Jan, Jan2, Mar }
            .AsQueryable()
            .MonthlyTotals(regs.AsQueryable(), checkIns.AsQueryable())
            .OrderBy(r => r.Month)
            .ToList();

        Assert.Equal(
            [new EventMonthRow(2026, 1, 2, 3, 1), new EventMonthRow(2026, 3, 1, 1, 2)],
            res
        );
    }

    [Fact]
    public void CountPerEvent_ForCheckIns_CountsLiveOnesOfTheRequestedEventsOnly()
    {
        var checkIns = new[] { CheckedIn(Jan), CheckedIn(Jan), CheckedIn(Jan, deleted: true), CheckedIn(Mar), CheckedIn(Jan2) };

        var res = checkIns.AsQueryable().CountPerEvent([Jan.Id, Mar.Id]).ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<Guid, int> { [Jan.Id] = 2, [Mar.Id] = 1 }, res);
    }
}
