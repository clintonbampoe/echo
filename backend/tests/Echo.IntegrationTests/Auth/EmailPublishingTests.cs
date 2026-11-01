using Echo.Application.Users;
using Echo.Auth.EmailVerifications;
using Echo.Auth.Passwords;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class EmailPublishingTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, User User)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, user);
    }

    [Fact]
    public async Task SendVerificationLink_QueuesOneEmailToTheUser_AndReturnsWithoutSending()
    {
        var (host, queue, user) = await Arrange();
        await using var _ = host;

        await host.Get<EmailVerificationService>()
            .SendVerificationLink(user, Ct)
            .WaitAsync(Patience, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal(user.EmailAddress, message.To);
        Assert.Equal("email_verification", message.Kind);
        Assert.Equal(user.Id, message.UserId);
        Assert.Equal(user.CongregationId, message.CongregationId);
        Assert.Contains("Verify", message.Content.Subject);
    }

    [Fact]
    public async Task SendVerificationLinkToEmail_UnknownEmail_QueuesNothing_SameMessage()
    {
        var (host, queue, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<EmailVerificationService>()
            .SendVerificationLinkToEmail("nobody@echo.test", Ct)
            .WaitAsync(Patience, Ct);

        Assert.Empty(queue.Messages);
        Assert.Equal("Email has been sent if user is valid", result.Message());
    }

    [Fact]
    public async Task SendVerificationLink_WithinSixtySeconds_QueuesNothingTheSecondTime()
    {
        var (host, queue, user) = await Arrange();
        await using var _ = host;
        var service = host.Get<EmailVerificationService>();

        await service.SendVerificationLink(user, Ct);
        var second = await service.SendVerificationLink(user, Ct);

        Assert.Single(queue.Messages);
        Assert.Contains("already sent", second.Message());
    }

    [Fact]
    public async Task SendForgotPasswordLink_QueuesOneEmail_AndReturnsWithoutSending()
    {
        var (host, queue, user) = await Arrange();
        await using var _ = host;

        var result = await host.Get<PasswordResetService>()
            .SendForgotPasswordLinkToEmail(user.EmailAddress, Ct)
            .WaitAsync(Patience, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal(user.EmailAddress, message.To);
        Assert.Equal("password_reset", message.Kind);
        Assert.Equal("Reset link has been sent.", result.Message());
    }

    [Fact]
    public async Task SendForgotPasswordLink_UnknownEmail_QueuesNothing_SameResultMessage()
    {
        var (host, queue, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<PasswordResetService>()
            .SendForgotPasswordLinkToEmail("nobody@echo.test", Ct)
            .WaitAsync(Patience, Ct);

        Assert.Empty(queue.Messages);
        Assert.Equal("Reset link has been sent.", result.Message());
    }
}
