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
    public async Task RegisterUser_QueuesOneVerificationEmail_WithoutExtraUserLookup()
    {
        var (host, queue, code) = await Arrange();
        await using var _ = host;
        host.Commands.Reset();

        await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct)
            .WaitAsync(Patience, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("kofi@echo.test", message.To);
        Assert.Equal("email_verification", message.Kind);
        Assert.Equal(6, host.Commands.Count); // was 7 before the user was no longer re-fetched by email
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
    public async Task RegisterCongregation_QueuesAVerificationEmailForTheAdmin()
    {
        var queue = new RecordingEmailQueue();
        await using var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(queue)
        );

        await host.Get<RegistrationService>()
            .RegisterCongregation(NewCongregation(), NewMember("admin@echo.test"), Ct)
            .WaitAsync(Patience, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("admin@echo.test", message.To);
        Assert.Equal("email_verification", message.Kind);
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
