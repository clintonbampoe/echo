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

    // User ids are time-ordered (UUIDv7), and the smallest id in a congregation is its creator.
    private static Guid IdFrom(TimeSpan ago) => Guid.CreateVersion7(DateTimeOffset.UtcNow - ago);

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, User Creator, User Joiner)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();

        var creator = TestHostExtensions.NewUser(congregationId, "ama@echo.test");
        creator.Id = IdFrom(TimeSpan.FromDays(2));
        var joiner = TestHostExtensions.NewUser(congregationId, "kofi@echo.test");
        joiner.FirstName = "Kofi";
        joiner.Role = UserRole.Clerk;
        joiner.Id = IdFrom(TimeSpan.Zero);

        host.Get<UserRepository>().Create(creator);
        host.Get<UserRepository>().Create(joiner);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, creator, joiner);
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
    public async Task VerifyingTheCreator_QueuesTheChurchReadyEmail()
    {
        var (host, queue, creator, _) = await Arrange();
        await using var _host = host;
        await AddVerificationToken(host, creator, "creator-token");

        var result = await host.Get<EmailVerificationService>().VerifyEmail("creator-token", Ct);

        Assert.IsNotType<InvalidTokenResult>(result);
        var message = Assert.Single(queue.Messages);
        Assert.Equal("church_ready", message.Kind);
        Assert.Equal(creator.EmailAddress, message.To);
        Assert.Equal(creator.Id, message.UserId);
        Assert.Contains("is ready on Echo", message.Content.Subject);
    }

    [Fact]
    public async Task VerifyingSomeoneWhoJoinedLater_QueuesTheWelcomeEmail()
    {
        var (host, queue, _, joiner) = await Arrange();
        await using var _host = host;
        await AddVerificationToken(host, joiner, "joiner-token");

        await host.Get<EmailVerificationService>().VerifyEmail("joiner-token", Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("welcome", message.Kind);
        Assert.Equal(joiner.EmailAddress, message.To);
        Assert.Equal("Your Echo account is verified", message.Content.Subject);
        Assert.Contains("Hi Kofi,", message.Content.HtmlBody);
    }

    [Fact]
    public async Task AnInvalidVerificationToken_QueuesNothing()
    {
        var (host, queue, _, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<EmailVerificationService>().VerifyEmail("no-such-token", Ct);

        Assert.IsType<InvalidTokenResult>(result);
        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task VerifyingWhenTheQueueRefuses_StillVerifiesTheAccount()
    {
        var (host, queue, creator, _) = await Arrange();
        await using var _host = host;
        queue.Accepting = false;
        await AddVerificationToken(host, creator, "creator-token");

        var result = await host.Get<EmailVerificationService>().VerifyEmail("creator-token", Ct);

        Assert.IsNotType<InvalidTokenResult>(result);
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
