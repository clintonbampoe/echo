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
