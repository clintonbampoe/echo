using Echo.Application.Attendances;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class AttendanceVisibilityTests
{
    private static Attendance NewAttendance(Session session)
    {
        var attendance = AttendanceFactory.NewEntity();
        attendance.Id = Guid.NewGuid();
        attendance.Session = session;
        attendance.SessionId = session.Id;
        return attendance;
    }

    private static Session NewSession(SessionStatus status = SessionStatus.Completed)
    {
        var session = SessionFactory.NewEntity();
        session.Id = Guid.NewGuid();
        session.Status = status;
        return session;
    }

    [Fact]
    public void Live_AttendanceOfALiveSession_IsKept()
    {
        var attendance = NewAttendance(NewSession());

        var res = new[] { attendance }.AsQueryable().Live().ToList();

        Assert.Single(res);
    }

    [Fact]
    public void Live_AttendanceOfADeletedSession_IsExcluded()
    {
        var session = NewSession();
        session.DeletedAt = DateTime.UtcNow;

        var res = new[] { NewAttendance(session) }.AsQueryable().Live().ToList();

        Assert.Empty(res);
    }

    [Fact]
    public void Live_DeletedAttendanceOfALiveSession_IsExcluded()
    {
        var attendance = NewAttendance(NewSession());
        attendance.DeletedAt = DateTime.UtcNow;

        var res = new[] { attendance }.AsQueryable().Live().ToList();

        Assert.Empty(res);
    }

    [Fact]
    public void InSessions_KeepsOnlyAttendanceOfThoseSessions()
    {
        var completed = NewSession(SessionStatus.Completed);
        var scheduled = NewSession(SessionStatus.Scheduled);
        var records = new[]
        {
            NewAttendance(completed),
            NewAttendance(completed),
            NewAttendance(scheduled),
        };
        var completedSessions = new[] { completed, scheduled }
            .AsQueryable()
            .Where(s => s.Status == SessionStatus.Completed);

        var res = records.AsQueryable().Live().InSessions(completedSessions).Count();

        Assert.Equal(2, res);
    }

    [Theory]
    [InlineData(0, 0, 0.0)]
    [InlineData(10, 0, 0.0)]
    [InlineData(6, 2, 3.0)]
    [InlineData(5, 2, 2.5)]
    public void AverageAttendance_DividesAttendanceByCompletedSessions_ZeroWhenNone(
        int attendance,
        int completedSessions,
        double expected
    )
    {
        Assert.Equal(expected, SessionService.AverageAttendance(attendance, completedSessions));
    }
}
