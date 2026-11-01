using Echo.Application.Users;
using Echo.Auth.EmailVerifications;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class TokenHygieneTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(AuthTestHost Host, User User)> Arrange()
    {
        var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(new RecordingEmailQueue())
        );
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, user);
    }

    [Fact]
    public async Task IssuingAVerificationToken_PrunesTheUsersDeadTokens_KeepsTheLiveOnes()
    {
        var (host, user) = await Arrange();
        await using var _ = host;
        var now = host.Time.GetUtcNow().UtcDateTime;
        EmailVerificationToken Token(string hash) =>
            new(user.Id)
            {
                TokenHash = hash,
                ExpiresAt = now.AddHours(1),
                CreatedAt = now.AddMinutes(-5),
            };
        var expired = Token("expired");
        expired.ExpiresAt = now.AddHours(-1);
        var used = Token("used");
        used.UsedAt = now.AddMinutes(-3);
        var oldInvalidated = Token("old-invalidated");
        oldInvalidated.InvalidatedAt = now.AddDays(-8);
        var live = Token("live");
        host.Get<AppDbContext>().AddRange(expired, used, oldInvalidated, live);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        await host.Get<EmailVerificationService>().SendVerificationLink(user, Ct);

        using var scope = host.NewScope();
        var left = await scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Set<EmailVerificationToken>()
            .AsNoTracking()
            .Where(t => t.UserId == user.Id)
            .ToListAsync(Ct);
        // the previous live token (now replaced) and the new one
        Assert.Equal(2, left.Count);
        Assert.Contains(left, t => t.TokenHash == "live" && t.InvalidatedAt is not null);
        Assert.DoesNotContain(left, t => t.TokenHash is "expired" or "used" or "old-invalidated");
    }

    [Fact]
    public async Task TwoVerificationTokensWithTheSameHash_AreRejectedByTheDatabase()
    {
        var (host, user) = await Arrange();
        await using var _ = host;
        var context = host.Get<AppDbContext>();
        context.Add(new EmailVerificationToken(user.Id) { TokenHash = "same" });
        context.Add(new EmailVerificationToken(user.Id) { TokenHash = "same" });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, ((PostgresException)ex.InnerException!).SqlState);
    }

    [Fact]
    public async Task TwoPasswordResetTokensWithTheSameHash_AreRejectedByTheDatabase()
    {
        var (host, user) = await Arrange();
        await using var _ = host;
        var context = host.Get<AppDbContext>();
        var expires = host.Time.GetUtcNow().UtcDateTime.AddHours(1);
        context.Add(new PasswordResetToken { UserId = user.Id, TokenHash = "same", ExpiresAt = expires });
        context.Add(new PasswordResetToken { UserId = user.Id, TokenHash = "same", ExpiresAt = expires });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(Ct));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, ((PostgresException)ex.InnerException!).SqlState);
    }
}
