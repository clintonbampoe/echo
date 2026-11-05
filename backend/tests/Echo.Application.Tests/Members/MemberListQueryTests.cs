using Echo.Application.Members;
using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Members;

// Builds the SQL without a database. Postgres prepares a statement after a few runs and then plans it once for any values of
// its parameters. For a range on two unknown dates it guesses that 0.5% of the rows match, so the members list reads the
// members by date and sorts them: over 100 ms for a big church (20,000 members) whenever the window is wide, against under
// 1 ms for the plan that reads the people in name order. For one unknown end it guesses a third, which keeps the good
// plan. So each end of the window must be its own condition.
[Trait("Category", "Unit")]
public class MemberListQueryTests
{
    private static AppDbContext Context()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        return new AppDbContext(builder.Options);
    }

    private static string Where(string sql) => sql[sql.IndexOf("WHERE", StringComparison.Ordinal)..];

    [Fact]
    public void MemberFilter_WithAWindow_TestsEachEndOnItsOwn_AllowingUndatedMembers()
    {
        using var db = Context();
        var filters = new MemberFilters { From = new DateOnly(2026, 1, 1), To = new DateOnly(2026, 12, 31) };

        var where = Where(db.Members.Filter(filters).ToQueryString());

        Assert.Contains("m.joined_date IS NULL OR m.joined_date >= @", where);
        Assert.Contains("m.joined_date IS NULL OR m.joined_date <= @", where);
    }

    [Fact]
    public void MemberFilter_WithAWindow_NeverTestsBothEndsInOneRange()
    {
        using var db = Context();
        var filters = new MemberFilters { From = new DateOnly(2026, 1, 1), To = new DateOnly(2026, 12, 31) };

        var where = Where(db.Members.Filter(filters).ToQueryString());

        Assert.DoesNotContain("BETWEEN", where);
        Assert.DoesNotMatch(@"joined_date\s*>=\s*@\w+\s+AND\s+\w+\.joined_date\s*<=", where);
    }

    [Fact]
    public void MemberFilter_WithOnlyAnEnd_StillBoundsTheStart()
    {
        using var db = Context();
        var filters = new MemberFilters { To = new DateOnly(2026, 6, 30) };

        var where = Where(db.Members.Filter(filters).ToQueryString());

        Assert.Contains("joined_date <= @", where);
        Assert.Contains("joined_date >= @", where);
    }

    [Fact]
    public void MemberFilter_WithoutAWindow_HasNoDateTest()
    {
        using var db = Context();

        var sql = db.Members.Filter(new MemberFilters()).ToQueryString();

        Assert.DoesNotContain("joined_date >=", sql);
        Assert.DoesNotContain("joined_date <=", sql);
    }
}
