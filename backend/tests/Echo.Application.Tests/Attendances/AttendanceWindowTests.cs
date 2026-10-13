using Echo.Application.Attendances;
using Echo.Domain.Attendances;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class AttendanceWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Attendance OnDate(DateOnly sessionDate)
    {
        var attendance = AttendanceFactory.NewEntity();
        attendance.Session.Date = sessionDate;
        return attendance;
    }

    [Fact]
    public void Filter_NoDates_AppliesNoDateRestriction()
    {
        var rows = new[] { OnDate(new DateOnly(2020, 1, 1)), OnDate(new DateOnly(2026, 10, 14)) };

        var res = rows.AsQueryable().Filter(new AttendanceFilters()).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            OnDate(new DateOnly(2026, 10, 10)),
            OnDate(new DateOnly(2026, 10, 11)),
            OnDate(new DateOnly(2026, 10, 14)),
            OnDate(new DateOnly(2026, 10, 15)),
        };
        var filters = new AttendanceFilters
        {
            From = new DateOnly(2026, 10, 11),
            To = new DateOnly(2026, 10, 14),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void ResolveWindow_Unscoped_DefaultsToThisWeekToDate()
    {
        var w = AttendanceService.ResolveWindow(new AttendanceFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 14), true),
            w
        );
    }

    [Fact]
    public void ResolveWindow_SessionIdSet_ReturnsNull()
    {
        var filters = new AttendanceFilters { SessionId = Constants.DefaultGuid };

        Assert.Null(AttendanceService.ResolveWindow(filters, _windows));
    }

    [Fact]
    public void ResolveWindow_PersonIdSet_ReturnsNull()
    {
        var filters = new AttendanceFilters { PersonId = Constants.DefaultGuid };

        Assert.Null(AttendanceService.ResolveWindow(filters, _windows));
    }
}
