using Echo.Application.Caching;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Congregations;
using Echo.Domain.Tithes;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Caching;

// Works out which congregations a save touches without a database.
[Trait("Category", "Unit")]
public class CacheEvictionInterceptorTests
{
    private static readonly Guid Alpha = Guid.NewGuid();
    private static readonly Guid Beta = Guid.NewGuid();

    private static AppDbContext Context()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        return new AppDbContext(builder.Options);
    }

    private static Tithe Tithe(Guid congregationId) =>
        new() { Id = Guid.NewGuid(), CongregationId = congregationId };

    [Fact]
    public void ChangedRows_NameTheirCongregation_OncePerCongregation()
    {
        using var db = Context();
        db.Add(Tithe(Alpha));
        db.Add(Tithe(Alpha));
        db.Add(Tithe(Beta));

        var tags = CacheEvictionInterceptor.TagsOf(db.ChangeTracker.Entries()).ToList();

        Assert.Equal(2, tags.Count);
        Assert.Contains(ReadCache.Tag(Alpha), tags);
        Assert.Contains(ReadCache.Tag(Beta), tags);
    }

    [Fact]
    public void ARowThatWasOnlyRead_ChangesNothing()
    {
        using var db = Context();
        db.Attach(Tithe(Alpha));

        Assert.Empty(CacheEvictionInterceptor.TagsOf(db.ChangeTracker.Entries()));
    }

    [Fact]
    public void ACongregation_IsItsOwnCongregation()
    {
        using var db = Context();
        var church = new Congregation { Id = Alpha };
        db.Add(church);

        Assert.Equal(
            [ReadCache.Tag(Alpha)],
            CacheEvictionInterceptor.TagsOf(db.ChangeTracker.Entries())
        );
    }

    [Fact]
    public void ALoginToken_BelongsToNoCongregation_SoSigningInDropsNothing()
    {
        using var db = Context();
        db.Add(new RefreshToken(Guid.NewGuid()));

        Assert.Empty(CacheEvictionInterceptor.TagsOf(db.ChangeTracker.Entries()));
    }

    // A row that belongs to no congregation never drops a cached answer. Every kind of row must belong to
    // one, bar the few named here, or what a person writes would stay hidden behind the cache.
    [Fact]
    public void EveryKindOfRow_BelongsToACongregation_OrIsNamedAsOneThatDoesNot()
    {
        using var db = Context();
        var belongsToNone = new[]
        {
            typeof(Congregation),
            typeof(RefreshToken),
            typeof(PasswordResetToken),
            typeof(EmailVerificationToken),
        };

        var strays = db
            .Model.GetEntityTypes()
            .Where(t => t.FindPrimaryKey() is not null && t.FindProperty("CongregationId") is null)
            .Select(t => t.ClrType)
            .Except(belongsToNone)
            .ToList();

        Assert.Empty(strays);
    }
}
