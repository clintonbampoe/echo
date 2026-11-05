using Echo.Application.Congregations;
using Echo.Application.Users;
using Echo.Auth.Invitations;
using Echo.Auth.Registrations;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.Services.Hashing;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class RegistrationEmailTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static UserCreateDto NewMember(string email = "kofi@echo.test") =>
        new()
        {
            FirstName = "Kofi",
            LastName = "Boateng",
            EmailAddress = email,
            Password = "correct horse battery",
        };

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, string InvitationCode)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();
        var admin = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(admin);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var invitation = await host.Get<InvitationService>()
            .CreateInvitationToken(
                congregationId,
                admin.Id,
                new InviteRequest { AllowedRole = UserRole.Member },
                Ct
            );
        return (host, queue, invitation.Value<InviteResponseDto>().Token);
    }

    [Fact]
    public async Task RegisterUser_QueuesTheVerificationAndWelcomeEmails_WithoutExtraUserLookup()
    {
        var (host, queue, code) = await Arrange();
        await using var _ = host;
        host.Commands.Reset();

        await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct)
            .WaitAsync(Patience, Ct);

        Assert.Equal(2, queue.Messages.Count);
        Assert.All(queue.Messages, m => Assert.Equal("kofi@echo.test", m.To));
        Assert.Equal("email_verification", queue.Messages[0].Kind);
        var welcome = queue.Messages[1];
        Assert.Equal("welcome", welcome.Kind);
        Assert.StartsWith("Welcome to Grace Chapel ", welcome.Content.Subject);
        Assert.Contains("Hi Kofi,", welcome.Content.HtmlBody);
        // 7 -> 6 when the user stopped being re-fetched, +1 for pruning old verification tokens,
        // +1 for reading the church's name for the welcome email.
        Assert.Equal(8, host.Commands.Count);
    }

    [Fact]
    public async Task RegisterUser_ReturnsWithoutWaitingForTheEmailService()
    {
        var (host, _, code) = await Arrange();
        await using var _host = host;

        var result = await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct)
            .WaitAsync(Patience, Ct);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task RegisterCongregation_QueuesTheVerificationAndSetUpEmailsForTheAdmin()
    {
        var queue = new RecordingEmailQueue();
        await using var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(queue)
        );

        await host.Get<RegistrationService>()
            .RegisterCongregation(NewCongregation(), NewMember("admin@echo.test"), Ct)
            .WaitAsync(Patience, Ct);

        Assert.Equal(2, queue.Messages.Count);
        Assert.All(queue.Messages, m => Assert.Equal("admin@echo.test", m.To));
        Assert.Equal("email_verification", queue.Messages[0].Kind);
        var setUp = queue.Messages[1];
        Assert.Equal("set_up_church", setUp.Kind);
        Assert.Equal("Set up Grace Chapel on Echo", setUp.Content.Subject);
        Assert.Contains("Add your members", setUp.Content.HtmlBody);
    }

    [Fact]
    public async Task RegisterCongregation_WhenTheQueueRefuses_StillCreatesTheAccount()
    {
        var queue = new RecordingEmailQueue { Accepting = false };
        await using var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(queue)
        );

        await host.Get<RegistrationService>()
            .RegisterCongregation(NewCongregation(), NewMember("admin@echo.test"), Ct);

        Assert.Empty(queue.Messages);
        Assert.NotNull(await host.Get<UserRepository>().GetByEmail("admin@echo.test", Ct));
    }

    [Fact]
    public async Task RegisterCongregation_StoresAHashedPassword_NotThePlaintext()
    {
        var queue = new RecordingEmailQueue();
        await using var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(queue)
        );

        await host.Get<RegistrationService>()
            .RegisterCongregation(NewCongregation(), NewMember("admin@echo.test"), Ct);

        var admin = await host.Get<UserRepository>().GetByEmail("admin@echo.test", Ct);
        Assert.NotNull(admin);
        Assert.NotEqual("correct horse battery", admin.PasswordHash);
        Assert.True(
            await host.Get<IPasswordHasher>().VerifyAsync("correct horse battery", admin.PasswordHash)
        );
    }

    private static CongregationCreateDto NewCongregation() =>
        new()
        {
            Name = "Grace Chapel",
            PhoneNumber = "0200000000",
            EmailAddress = "office@echo.test",
            City = "Accra",
            Town = "Osu",
            GpsAddress = "GA-000-0000",
        };
}
