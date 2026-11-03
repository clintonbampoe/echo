using Echo.Application.Attendances;
using Echo.Domain.Attendances;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionAttendanceTests
{
    private static Attendance Row(
        Guid sessionId,
        PersonKind kind,
        bool first = false,
        bool deleted = false
    )
    {
        var attendance = TestData.Shared.CreateAttendance();
        attendance.Id = Guid.NewGuid();
        attendance.SessionId = sessionId;
        attendance.PersonKindAtTime = kind;
        attendance.IsFirstVisit = first;
        attendance.DeletedAt = deleted ? DateTime.UtcNow : null;
        return attendance;
    }

    [Fact]
    public void AttendancePerSession_SplitsEachRequestedSessionIntoMembersVisitorsAndFirstTimers()
    {
        var sunday = Guid.NewGuid();
        var midweek = Guid.NewGuid();
        var other = Guid.NewGuid();
        var rows = new[]
        {
            Row(sunday, PersonKind.Member),
            Row(sunday, PersonKind.Member),
            Row(sunday, PersonKind.Visitor, first: true),
            Row(sunday, PersonKind.Visitor),
            Row(sunday, PersonKind.Member, deleted: true),
            Row(midweek, PersonKind.Member),
            Row(other, PersonKind.Member),
        };

        var res = rows.AsQueryable().AttendancePerSession([sunday, midweek]).ToDictionary(x => x.Id);

        Assert.Equal(new SessionAttendance(sunday, 4, 2, 2, 1), res[sunday]);
        Assert.Equal(new SessionAttendance(midweek, 1, 1, 0, 0), res[midweek]);
        Assert.False(res.ContainsKey(other));
    }

    private static Session OnDay(DateOnly date) => new() { Id = Guid.NewGuid(), Date = date };

    [Fact]
    public void SettledIds_AreTheSessionsFromBeforeYesterday_NotYesterdayOrLater()
    {
        var today = new DateOnly(2026, 11, 3);
        var lastWeek = OnDay(today.AddDays(-7));
        var dayBefore = OnDay(today.AddDays(-2));
        var yesterday = OnDay(today.AddDays(-1));
        var now = OnDay(today);
        var upcoming = OnDay(today.AddDays(5));

        var settled = SessionRepository.SettledIds([lastWeek, dayBefore, yesterday, now, upcoming], today);

        Assert.Equal([lastWeek.Id, dayBefore.Id], settled);
    }

    [Fact]
    public void SettledAttendance_ReadsTheViewRowsOfTheRequestedSessionsOnly()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var other = Guid.NewGuid();
        var rows = new[]
        {
            new SessionAttendanceTotals { SessionId = a, Total = 40, Members = 30, Visitors = 10, FirstTime = 2 },
            new SessionAttendanceTotals { SessionId = b, Total = 5, Members = 5, Visitors = 0, FirstTime = 0 },
            new SessionAttendanceTotals { SessionId = other, Total = 9, Members = 9, Visitors = 0, FirstTime = 0 },
        };

        var res = rows.AsQueryable().SettledAttendance([a, b]).ToDictionary(x => x.Id);

        Assert.Equal(new SessionAttendance(a, 40, 30, 10, 2), res[a]);
        Assert.Equal(new SessionAttendance(b, 5, 5, 0, 0), res[b]);
        Assert.False(res.ContainsKey(other));
    }

    [Fact]
    public void WithAttendance_FillsTheCountsOnTheDto_AndLeavesASessionWithNoAttendanceAtZero()
    {
        var id = Guid.NewGuid();
        var dto = new SessionResponseDto { Id = id, SessionLabelName = "Sunday" };
        var counts = new Dictionary<Guid, SessionAttendance> { [id] = new(id, 5, 3, 2, 1) };

        var filled = SessionService.WithAttendance(dto, counts);
        var empty = SessionService.WithAttendance(dto with { Id = Guid.NewGuid() }, counts);

        Assert.Equal((5, 3, 2, 1), (filled.AttendanceCount, filled.MembersPresent, filled.VisitorsPresent, filled.FirstTimeVisitors));
        Assert.Equal((0, 0, 0, 0), (empty.AttendanceCount, empty.MembersPresent, empty.VisitorsPresent, empty.FirstTimeVisitors));
    }
}
