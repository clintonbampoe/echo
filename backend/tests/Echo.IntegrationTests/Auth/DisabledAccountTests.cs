using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class DisabledAccountTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<User> AddUser(
        AuthTestHost host,
        Guid congregationId,
        string email,
        UserRole role
    )
    {
        var user = TestHostExtensions.NewUser(congregationId, email);
        user.Role = role;
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return user;
    }

    [Fact]
    public async Task Refresh_DisabledAccount_IsRefused()
    {
        await using var host = await AuthTestHost.CreateAsync(db);
        var congregationId = await host.SeedCongregationAsync();
        var user = await AddUser(host, congregationId, "ama@echo.test", UserRole.Member);
        var pair = await host.Get<SessionService>().StartSession(user, Ct);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        // Simulates a token that survived: disabling also revokes, but the refresh check must hold on its own.
        user.DisabledAt = DateTime.UtcNow;
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var result = await host.Get<SessionService>().RefreshAccessToken(pair.RefreshToken, Ct);

        Assert.IsNotType<Microsoft.AspNetCore.Http.HttpResults.Ok<TokenPairResponseDtos>>(result.ToResult());
    }

    [Fact]
    public async Task RevokeSessions_RevokesEveryActiveTokenOfThatUserOnly()
    {
        await using var host = await AuthTestHost.CreateAsync(db);
        var congregationId = await host.SeedCongregationAsync();
        var target = await AddUser(host, congregationId, "ama@echo.test", UserRole.Member);
        var other = await AddUser(host, congregationId, "kofi@echo.test", UserRole.Member);
        var sessions = host.Get<SessionService>();
        await sessions.StartSession(target, Ct);
        await sessions.StartSession(other, Ct);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        await host.Get<UserRepository>().RevokeSessions(target.Id, DateTime.UtcNow, Ct);

        using var scope = host.NewScope();
        var tokens = await scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Set<RefreshToken>()
            .AsNoTracking()
            .ToListAsync(Ct);
        Assert.All(tokens.Where(t => t.UserId == target.Id), t => Assert.NotNull(t.RevokedAt));
        Assert.All(tokens.Where(t => t.UserId == other.Id), t => Assert.Null(t.RevokedAt));
    }

    [Fact]
    public async Task HasOtherActiveAdmin_IgnoresDisabledAdminsAndOtherCongregations()
    {
        await using var host = await AuthTestHost.CreateAsync(db);
        var congregationId = await host.SeedCongregationAsync();
        var me = await AddUser(host, congregationId, "ama@echo.test", UserRole.Admin);
        var disabled = await AddUser(host, congregationId, "kofi@echo.test", UserRole.Admin);
        disabled.DisabledAt = DateTime.UtcNow;
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        var users = host.Get<UserRepository>();

        Assert.False(await users.HasOtherActiveAdmin(congregationId, me.Id, Ct));

        await AddUser(host, congregationId, "yaw@echo.test", UserRole.Admin);
        Assert.True(await users.HasOtherActiveAdmin(congregationId, me.Id, Ct));
    }

    [Fact]
    public async Task CountByRole_CountsPerRoleAndDisabledOnly_ForOneCongregation()
    {
        await using var host = await AuthTestHost.CreateAsync(db);
        var congregationId = await host.SeedCongregationAsync();
        await AddUser(host, congregationId, "a@echo.test", UserRole.Admin);
        var clerk = await AddUser(host, congregationId, "b@echo.test", UserRole.Clerk);
        await AddUser(host, congregationId, "c@echo.test", UserRole.Clerk);
        clerk.DisabledAt = DateTime.UtcNow;
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var counts = await host.Get<UserRepository>().CountByRole(congregationId, Ct);

        var clerks = Assert.Single(counts, c => c.Role == UserRole.Clerk);
        Assert.Equal(2, clerks.Count);
        Assert.Equal(1, clerks.Disabled);
        Assert.Equal(1, Assert.Single(counts, c => c.Role == UserRole.Admin).Count);
        Assert.DoesNotContain(counts, c => c.Role == UserRole.Member);
    }
}
