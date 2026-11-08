using Echo.Application.Attendances;
using Echo.Application.Caching;
using Echo.Application.Tests.Caching;
using Echo.Data;
using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Attendances;

// Builds the SQL without a database, to check which table the summaries read. The view holds one
// row per session; the attendance table holds one per check-in, and a year of a big congregation
// is hundreds of thousands of them.
[Trait("Category", "Unit")]
public class AttendanceTotalsQueryTests
{
    private static AppDbContext Context()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        return new AppDbContext(builder.Options);
    }

    private static string SummarySql(AttendanceFilters filters)
    {
        using var db = Context();
        return new AttendanceRepository(db, new ReadCacheEvictions(new RecordingCacheStore())).SummaryQuery(Guid.NewGuid(), filters).ToQueryString();
    }

    [Fact]
    public void Summary_ByDateAndLabel_ReadsTheViewNotTheCheckIns()
    {
        var sql = SummarySql(
            new AttendanceFilters
            {
                SessionLabelId = 2,
                From = new DateOnly(2026, 1, 1),
                To = new DateOnly(2026, 10, 1),
            }
        );

        Assert.Contains("attendance_per_session", sql);
        Assert.DoesNotContain("FROM attendance ", sql);
    }

    [Fact]
    public void Summary_ForOnePerson_ReadsTheCheckIns()
    {
        var sql = SummarySql(new AttendanceFilters { PersonId = Guid.NewGuid() });

        Assert.DoesNotContain("attendance_per_session", sql);
    }

    [Fact]
    public void Summary_ForOneKind_ReadsTheCheckIns()
    {
        var sql = SummarySql(new AttendanceFilters { Kind = PersonKind.Visitor });

        Assert.DoesNotContain("attendance_per_session", sql);
    }

    [Fact]
    public void Summary_ForOneSession_ReadsTheCheckIns()
    {
        var sql = SummarySql(new AttendanceFilters { SessionId = Guid.NewGuid() });

        Assert.DoesNotContain("attendance_per_session", sql);
    }

    [Fact]
    public void Summary_ByName_ReadsTheCheckIns()
    {
        var sql = SummarySql(new AttendanceFilters { Name = "abb" });

        Assert.DoesNotContain("attendance_per_session", sql);
    }

    [Fact]
    public void Monthly_ReadsTheView()
    {
        using var db = Context();

        var sql = new AttendanceRepository(db, new ReadCacheEvictions(new RecordingCacheStore()))
            .MonthlyQuery(Guid.NewGuid(), 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 10, 31))
            .ToQueryString();

        Assert.Contains("attendance_per_session", sql);
    }
}
