using Echo.Application.Events;
using Echo.Domain.Events;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventCountsListTests
{
    private static Event EndingOn(DateOnly date) => new() { Id = Guid.NewGuid(), StartDate = date, EndDate = date };

    [Fact]
    public void SettledIds_AreTheEventsThatEndedBeforeYesterday_NotYesterdayOrLater()
    {
        var today = new DateOnly(2026, 11, 3);
        var longAgo = EndingOn(today.AddDays(-90));
        var dayBefore = EndingOn(today.AddDays(-2));
        var yesterday = EndingOn(today.AddDays(-1));
        var now = EndingOn(today);
        var upcoming = EndingOn(today.AddDays(14));

        var settled = EventRepository.SettledIds([longAgo, dayBefore, yesterday, now, upcoming], today);

        Assert.Equal([longAgo.Id, dayBefore.Id], settled);
    }

    [Fact]
    public void SettledIds_UsesTheEndDate_SoALongEventStillRunningIsCountedLive()
    {
        var today = new DateOnly(2026, 11, 3);
        var camp = new Event
        {
            Id = Guid.NewGuid(),
            StartDate = today.AddDays(-10),
            EndDate = today.AddDays(2),
        };

        Assert.Empty(EventRepository.SettledIds([camp], today));
    }
}
