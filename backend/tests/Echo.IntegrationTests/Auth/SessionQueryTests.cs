using Echo.Application.Users;
using Echo.Auth.Passwords;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class SessionQueryTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(AuthTestHost Host, User User, TokenPairResponseDtos Pair)> Arrange(
        FailingSaveInterceptor? failing = null
    )
    {
        var host = await AuthTestHost.CreateAsync(
            db,
            services =>
            {
                if (failing is not null)
                    services.AddSingleton<ISaveChangesInterceptor>(failing);
            }
        );
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        var pair = await StartSession(host, user);
        return (host, user, pair);
    }

    private static async Task<TokenPairResponseDtos> StartSession(AuthTestHost host, User user)
    {
        var pair = await host.Get<SessionService>().StartSession(user, Ct);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return pair;
    }

    /// <summary>Reads tokens through a fresh context, so tracked entities cannot hide the database state.</summary>
    private static async Task<List<RefreshToken>> TokensOf(AuthTestHost host, Guid userId)
    {
        using var scope = host.NewScope();
        return await scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Set<RefreshToken>()
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(Ct);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesWithThreeCommands()
    {
        var (host, user, pair) = await Arrange();
        await using var _ = host;
        host.Commands.Reset();

        var result = await host.Get<SessionService>().RefreshAccessToken(pair.RefreshToken, Ct);

        // select token + user, prune old tokens, one commit (new token inserted, old one revoked)
        Assert.Equal(3, host.Commands.Count);
        var rotated = result.Value<TokenPairResponseDtos>();
        Assert.NotEqual(pair.RefreshToken, rotated.RefreshToken);
        var tokens = await TokensOf(host, user.Id);
        Assert.Equal(2, tokens.Count);
        Assert.Single(tokens, t => t.RevokedAt is null);
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesEverySession()
    {
        var (host, user, pair) = await Arrange();
        await using var _ = host;
        var sessions = host.Get<SessionService>();
        await sessions.RefreshAccessToken(pair.RefreshToken, Ct); // rotates: pair's token is now revoked
        host.Time.Advance(TimeSpan.FromSeconds(11)); // well past the grace window for a second tab

        var replay = await sessions.RefreshAccessToken(pair.RefreshToken, Ct);

        Assert.IsType<BadRequestResult>(replay);
        var tokens = await TokensOf(host, user.Id);
        Assert.All(tokens, t => Assert.NotNull(t.RevokedAt));
    }

    [Fact]
    public async Task Refresh_RotatedTokenReplayedWithinTenSeconds_IsRefused_ButKeepsTheSessions()
    {
        // A second browser tab shares the stored tokens and may present the one the first tab has
        // just rotated. That is a race, not theft: refuse it without signing everyone out.
        var (host, user, pair) = await Arrange();
        await using var _ = host;
        var sessions = host.Get<SessionService>();
        await sessions.RefreshAccessToken(pair.RefreshToken, Ct);
        host.Time.Advance(TimeSpan.FromSeconds(5));

        var replay = await sessions.RefreshAccessToken(pair.RefreshToken, Ct);

        Assert.IsType<BadRequestResult>(replay);
        var tokens = await TokensOf(host, user.Id);
        Assert.Single(tokens, t => t.RevokedAt is null);
    }

    [Fact]
    public async Task RevokeToken_RevokesWithoutLoadingTheUser()
    {
        var (host, user, pair) = await Arrange();
        await using var _ = host;
        host.Commands.Reset();

        await host.Get<SessionService>().RevokeAccessToken(pair.RefreshToken, Ct);

        Assert.Equal(2, host.Commands.Count); // select the token, update it
        Assert.DoesNotContain(host.Commands.Statements, sql => sql.Contains("JOIN", StringComparison.OrdinalIgnoreCase));
        Assert.All(await TokensOf(host, user.Id), t => Assert.NotNull(t.RevokedAt));
    }

    [Fact]
    public async Task RevokeAllActiveSessions_UsesOneStatement_AndRevokesOnlyActiveTokens()
    {
        var (host, user, first) = await Arrange();
        await using var _ = host;
        await StartSession(host, user); // a second active session
        await host.Get<SessionService>().RevokeAccessToken(first.RefreshToken, Ct);
        var alreadyRevoked = (await TokensOf(host, user.Id)).Single(t => t.RevokedAt is not null);
        host.Time.Advance(TimeSpan.FromHours(1));
        host.Commands.Reset();

        await host.Get<JwtTokenService>().RevokeAllActiveSessionsForUserById(user.Id, Ct);

        Assert.Equal(1, host.Commands.Count);
        var tokens = await TokensOf(host, user.Id);
        Assert.All(tokens, t => Assert.NotNull(t.RevokedAt));
        Assert.Equal(
            alreadyRevoked.RevokedAt,
            tokens.Single(t => t.Id == alreadyRevoked.Id).RevokedAt
        );
    }

    [Fact]
    public async Task ResetPassword_RevokesSessions_AndChangesThePassword()
    {
        var (host, user, _) = await Arrange();
        await using var _host = host;
        await AddResetToken(host, user, "reset-token-1");

        var result = await host.Get<PasswordResetService>().ResetPassword("reset-token-1", "brand new password", Ct);

        Assert.IsAssignableFrom<IOperationResult>(result);
        Assert.All(await TokensOf(host, user.Id), t => Assert.NotNull(t.RevokedAt));
        using var scope = host.NewScope();
        var saved = await scope.ServiceProvider.GetRequiredService<UserRepository>().GetByEmail(user.EmailAddress, Ct);
        Assert.True(await host.Get<IPasswordHasher>().VerifyAsync("brand new password", saved!.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenTheSaveFails_DoesNotRevokeSessions()
    {
        var failing = new FailingSaveInterceptor();
        var (host, user, _) = await Arrange(failing);
        await using var _host = host;
        await AddResetToken(host, user, "reset-token-2");
        failing.Fail = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            host.Get<PasswordResetService>().ResetPassword("reset-token-2", "brand new password", Ct)
        );

        Assert.All(await TokensOf(host, user.Id), t => Assert.Null(t.RevokedAt));
    }

    private static async Task AddResetToken(AuthTestHost host, User user, string token)
    {
        await host.Get<PasswordResetRepository>()
            .CreateRecord(
                new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = host.Get<ITokenHasher>().Hash(token),
                    ExpiresAt = host.Time.GetUtcNow().UtcDateTime.AddHours(1),
                },
                Ct
            );
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
    }
}
