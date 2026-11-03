using Echo.Application.Members;
using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Members;

// Builds the SQL without a database. The visitor list shows the inviter's name only, and that is read in one small query
// for the whole page. Loading the inviter with every visitor added the inviter's member row and person row (about 30
// columns) to each of the 50 rows, and about half of the time the list took.
[Trait("Category", "Unit")]
public class VisitorListQueryTests
{
    private static AppDbContext Context()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        return new AppDbContext(builder.Options);
    }

    [Fact]
    public void VisitorList_ReadsTheVisitorAndItsPerson_ButNotTheInviter()
    {
        using var db = Context();

        var sql = db.Visitors.ListScope(Guid.NewGuid(), new VisitorFilters(), null, 51).ToQueryString();

        Assert.Contains("INNER JOIN people AS p", sql);
        Assert.DoesNotContain("members", sql);
    }

    [Fact]
    public void VisitorList_ReadsPeopleInNameOrder_AndOneRowMoreThanThePage()
    {
        using var db = Context();

        var sql = db.Visitors.ListScope(Guid.NewGuid(), new VisitorFilters(), null, 51).ToQueryString();

        Assert.Contains("ORDER BY p.name, v.person_id", sql);
        Assert.Contains("-- @p='51'", sql);
    }
}
