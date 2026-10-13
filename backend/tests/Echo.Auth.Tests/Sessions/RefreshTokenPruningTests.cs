using Echo.Auth.Sessions;
using Echo.Domain.Auth;

namespace Echo.Auth.Tests.Sessions;

[Trait("Category", "Unit")]
public class RefreshTokenPruningTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private static RefreshToken Token(DateTime expiresAt, DateTime? revokedAt = null) =>
        new(Guid.NewGuid())
        {
            Id = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
        };

    [Fact]
    public void Prunable_KeepsActiveTokens()
    {
        var active = Token(Now.AddDays(20));

        Assert.Empty(new[] { active }.AsQueryable().Prunable(Now));
    }

    [Fact]
    public void Prunable_IncludesTokensThatHaveExpired()
    {
        var expired = Token(Now.AddMinutes(-1));

        Assert.Single(new[] { expired }.AsQueryable().Prunable(Now));
    }

    [Fact]
    public void Prunable_IncludesTokensRevokedMoreThanSevenDaysAgo()
    {
        var oldRevoked = Token(Now.AddDays(20), Now.AddDays(-8));

        Assert.Single(new[] { oldRevoked }.AsQueryable().Prunable(Now));
    }

    [Fact]
    public void Prunable_KeepsRecentlyRevokedTokens()
    {
        var recentlyRevoked = Token(Now.AddDays(20), Now.AddDays(-2));

        Assert.Empty(new[] { recentlyRevoked }.AsQueryable().Prunable(Now));
    }
}
