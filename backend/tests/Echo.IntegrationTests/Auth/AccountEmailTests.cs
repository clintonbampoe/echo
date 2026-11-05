using Echo.Application.Users;
using Echo.Auth.EmailVerifications;
using Echo.Auth.Profile;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;

namespace Echo.IntegrationTests.Auth;

/// <summary>The emails that follow verifying an account and changing a password.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class AccountEmailTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, User User)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();

        var user = TestHostExtensions.NewUser(congregationId, "ama@echo.test");
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, user);
    }

    private static async Task AddVerificationToken(AuthTestHost host, User user, string token)
    {
        var now = host.Time.GetUtcNow().UtcDateTime;
        host.Get<AppDbContext>()
            .Add(
                new EmailVerificationToken(user.Id)
                {
                    TokenHash = host.Get<ITokenHasher>().Hash(token),
                    ExpiresAt = now.AddHours(1),
                    CreatedAt = now,
                }
            );
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
    }

    [Fact]
    public async Task VerifyingAnAccount_QueuesNoFurtherEmail()
    {
        var (host, queue, user) = await Arrange();
        await using var _host = host;
        await AddVerificationToken(host, user, "the-token");

        var result = await host.Get<EmailVerificationService>().VerifyEmail("the-token", Ct);

        Assert.IsNotType<InvalidTokenResult>(result);
        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task AnInvalidVerificationToken_QueuesNothing()
    {
        var (host, queue, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<EmailVerificationService>().VerifyEmail("no-such-token", Ct);

        Assert.IsType<InvalidTokenResult>(result);
        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task ChangingThePasswordInTheProfile_QueuesAPasswordChangedEmail()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        await using var _host = host;
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        user.PasswordHash = await host.Get<IPasswordHasher>().HashAsync("old password here", Ct);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        await host.Get<ProfileService>()
            .Update(
                congregationId,
                user.Id,
                new ProfileUpdateDto
                {
                    CurrentPassword = "old password here",
                    NewPassword = "brand new password",
                },
                Ct
            );

        var message = Assert.Single(queue.Messages);
        Assert.Equal("password_changed", message.Kind);
        Assert.Equal(user.EmailAddress, message.To);
        Assert.Equal("Your Echo password was changed", message.Content.Subject);
    }

    [Fact]
    public async Task ChangingOnlyTheName_QueuesNoEmail()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        await using var _host = host;
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        await host.Get<ProfileService>()
            .Update(congregationId, user.Id, new ProfileUpdateDto { FirstName = "Abena" }, Ct);

        Assert.Empty(queue.Messages);
    }
}
