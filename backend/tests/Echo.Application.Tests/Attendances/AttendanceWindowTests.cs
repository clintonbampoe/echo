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
        var attendance = TestData.Shared.CreateAttendance();
        attendance.SessionDate = sessionDate;
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
    public void Paginate_Cursor_KeepsRowsBeforeItNewestFirstWithinADate()
    {
        var cursorId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        var sameDateOlderId = OnDate(new DateOnly(2026, 10, 11));
        sameDateOlderId.Id = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var sameDateNewerId = OnDate(new DateOnly(2026, 10, 11));
        sameDateNewerId.Id = Guid.Parse("00000000-0000-0000-0000-000000000006");
        var earlier = OnDate(new DateOnly(2026, 10, 4));
        var later = OnDate(new DateOnly(2026, 10, 18));
        var cursor = new AttendanceCursor { SessionDate = new DateOnly(2026, 10, 11), Id = cursorId };

        var res = new[] { sameDateOlderId, sameDateNewerId, earlier, later }
            .AsQueryable()
            .Paginate(cursor, 10)
            .ToList();

        Assert.Equal([sameDateOlderId, earlier], res);
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
        var filters = new AttendanceFilters { SessionId = TestData.DefaultId };

        Assert.Null(AttendanceService.ResolveWindow(filters, _windows));
    }

    [Fact]
    public void ResolveWindow_PersonIdSet_ReturnsNull()
    {
        var filters = new AttendanceFilters { PersonId = TestData.DefaultId };

        Assert.Null(AttendanceService.ResolveWindow(filters, _windows));
    }

    [Fact]
    public void MonthlyTotals_SplitsEachMonthIntoMembersVisitorsAndFirstTimeVisitors()
    {
        Attendance Row(DateOnly date, Echo.Domain.Members.PersonKind kind, bool first = false)
        {
            var attendance = OnDate(date);
            attendance.PersonKindAtTime = kind;
            attendance.IsFirstVisit = first;
            return attendance;
        }

        var member = Echo.Domain.Members.PersonKind.Member;
        var visitor = Echo.Domain.Members.PersonKind.Visitor;
        var rows = new[]
        {
            Row(new DateOnly(2026, 3, 1), member),
            Row(new DateOnly(2026, 3, 8), member),
            Row(new DateOnly(2026, 3, 8), visitor, first: true),
            Row(new DateOnly(2026, 3, 15), visitor),
            Row(new DateOnly(2026, 5, 3), member),
        };

        var res = rows.AsQueryable().MonthlyTotals().OrderBy(r => r.Month).ToList();

        Assert.Equal(
            [
                new AttendanceMonthlyDto { Year = 2026, Month = 3, TotalPresent = 4, MembersPresent = 2, VisitorsPresent = 2, FirstTimeVisitors = 1 },
                new AttendanceMonthlyDto { Year = 2026, Month = 5, TotalPresent = 1, MembersPresent = 1 },
            ],
            res
        );
    }
}
