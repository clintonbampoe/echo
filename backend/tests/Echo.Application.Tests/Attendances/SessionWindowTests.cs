using Echo.Application.Attendances;
using Echo.Domain.Attendances;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Session On(DateOnly date)
    {
        var session = SessionFactory.NewEntity();
        session.Date = date;
        return session;
    }

    [Fact]
    public void Filter_NoDates_AppliesNoDateRestriction()
    {
        var rows = new[] { On(new DateOnly(2020, 1, 1)), On(new DateOnly(2026, 10, 14)) };

        var res = rows.AsQueryable().Filter(new SessionFilters()).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void ResolveWindow_Default_IncludesUpcomingSessionLaterThisWeek()
    {
        var w = SessionService.ResolveWindow(new SessionFilters(), _windows);
        var filters = new SessionFilters { From = w?.From, To = w?.To };
        var tomorrow = On(new DateOnly(2026, 10, 15));
        var nextWeek = On(new DateOnly(2026, 10, 18));

        var res = new[] { tomorrow, nextWeek }.AsQueryable().Filter(filters).ToList();

        Assert.Equal(new DateOnly(2026, 10, 11), w!.Value.From);
        Assert.Equal(new DateOnly(2026, 10, 17), w.Value.To);
        var only = Assert.Single(res);
        Assert.Equal(tomorrow.Date, only.Date);
    }
}
