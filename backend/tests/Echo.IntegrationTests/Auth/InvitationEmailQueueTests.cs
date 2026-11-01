using Echo.Application.Users;
using Echo.Auth.Invitations;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class InvitationEmailQueueTests(PostgresFixture db)
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly InviteByEmailRequest Request = new()
    {
        EmailAddress = "kofi@echo.test",
        AllowedRole = UserRole.Member,
    };

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, User Admin)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();
        var admin = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(admin);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, admin);
    }

    [Fact]
    public async Task CreateAndEmail_QueuesTheInvitationEmail_AndReportsEmailedTo()
    {
        var (host, queue, admin) = await Arrange();
        await using var _ = host;

        var result = await host.Get<InvitationService>()
            .CreateAndEmail(admin.CongregationId, admin.Id, Request, Ct)
            .WaitAsync(Patience, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("kofi@echo.test", message.To);
        Assert.Equal("invitation", message.Kind);
        Assert.Equal(admin.CongregationId, message.CongregationId);
        var dto = result.Value<InviteResponseDto>();
        Assert.Equal("kofi@echo.test", dto.EmailedTo);
        Assert.False(string.IsNullOrEmpty(dto.Token));
        Assert.Contains(dto.Token, message.Content.HtmlBody);
    }

    [Fact]
    public async Task CreateAndEmail_WhenQueueRefuses_StillCreatesTheInvitation_WithNullEmailedTo()
    {
        var (host, queue, admin) = await Arrange();
        await using var _ = host;
        queue.Accepting = false;

        var result = await host.Get<InvitationService>()
            .CreateAndEmail(admin.CongregationId, admin.Id, Request, Ct)
            .WaitAsync(Patience, Ct);

        var dto = result.Value<InviteResponseDto>();
        Assert.Null(dto.EmailedTo);
        Assert.False(string.IsNullOrEmpty(dto.Token));
        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task CreateAndEmail_ReturnsWithoutWaitingForTheEmailService()
    {
        var (host, _, admin) = await Arrange();
        await using var _host = host;

        var result = await host.Get<InvitationService>()
            .CreateAndEmail(admin.CongregationId, admin.Id, Request, Ct)
            .WaitAsync(Patience, Ct);

        Assert.NotNull(result.Value<InviteResponseDto>().EmailedTo);
    }
}
