using Echo.Application.Members;
using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Members;

// Builds the SQL without a database, to check what the search asks Postgres to do. A trigram index can
// only help when the term has at least three characters; a shorter "contains" match reads every row.
[Trait("Category", "Unit")]
public class NameSearchQueryTests
{
    private static AppDbContext Context()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        return new AppDbContext(builder.Options);
    }

    [Fact]
    public void MemberSearch_WithThreeCharacters_MatchesAnywhereInTheName()
    {
        using var db = Context();

        var sql = db.Members.SearchName("abb").ToQueryString();

        Assert.Contains("'%abb%'", sql);
    }

    [Fact]
    public void MemberSearch_WithTwoCharacters_MatchesTheStartOfAWordSoTheIndexCanBeUsed()
    {
        using var db = Context();

        var sql = db.Members.SearchName("ab").ToQueryString();

        Assert.Contains("'ab%'", sql);
        Assert.Contains("'% ab%'", sql);
        Assert.DoesNotContain("'%ab%'", sql);
    }

    [Fact]
    public void VisitorSearch_WithTwoCharacters_MatchesTheStartOfAWordSoTheIndexCanBeUsed()
    {
        using var db = Context();

        var sql = db.Visitors.SearchName("ab").ToQueryString();

        Assert.Contains("'ab%'", sql);
        Assert.Contains("'% ab%'", sql);
        Assert.DoesNotContain("'%ab%'", sql);
    }

    [Fact]
    public void MemberSearch_WithTwoCharacters_ListsNamesThatStartWithThemFirst_NotBySimilarity()
    {
        using var db = Context();

        var sql = db.Members.SearchName("ab").ToQueryString();

        Assert.DoesNotContain("similarity(", sql);
        Assert.Contains("ORDER BY", sql);
    }

    [Fact]
    public void MemberSearch_WithThreeCharacters_StillRanksBySimilarity()
    {
        using var db = Context();

        var sql = db.Members.SearchName("abb").ToQueryString();

        Assert.Contains("similarity(", sql);
    }

    [Fact]
    public void MemberSearch_AsksTheNameIndexForThisCongregationsPeople()
    {
        using var db = Context();

        var sql = new MemberRepository(db, TimeProvider.System)
            .SearchQuery(Guid.NewGuid(), "ab")
            .ToQueryString();

        // Once for the member, once for the person.
        Assert.True(
            System.Text.RegularExpressions.Regex.Matches(sql, @"congregation_id = @").Count >= 2,
            sql
        );
    }
}
