using Echo.Application.Users;
using Echo.Auth.Invitations;
using Echo.Auth.Registrations;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class RegistrationAtomicityTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static UserCreateDto NewMember(string email) =>
        new()
        {
            FirstName = "Kofi",
            LastName = "Boateng",
            EmailAddress = email,
            Password = "correct horse battery",
        };

    private async Task<(AuthTestHost Host, string Code, Guid InvitationId, Guid CongregationId)> Arrange(
        Action<IServiceCollection> configure,
        int? maxUses = null
    )
    {
        var host = await AuthTestHost.CreateAsync(db, configure);
        var congregationId = await host.SeedCongregationAsync();
        var admin = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(admin);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        var result = await host.Get<InvitationService>()
            .CreateInvitationToken(
                congregationId,
                admin.Id,
                new InviteRequest { AllowedRole = UserRole.Member, MaxUses = maxUses },
                Ct
            );
        var dto = result.Value<InviteResponseDto>();
        return (host, dto.Token, dto.Id, congregationId);
    }

    private static async Task<InvitationToken> InvitationNow(AuthTestHost host, Guid id)
    {
        using var scope = host.NewScope();
        return await scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Set<InvitationToken>()
            .AsNoTracking()
            .SingleAsync(i => i.Id == id, Ct);
    }

    private static string Detail(IOperationResult result) =>
        ((ProblemHttpResult)result.ToResult()).ProblemDetails.Detail!;

    [Fact]
    public async Task RegisterUser_WhenUserInsertFails_DoesNotConsumeAnInvitationUse()
    {
        var failing = new FailingSaveInterceptor();
        var (host, code, invitationId, _) = await Arrange(s =>
            s.AddSingleton<ISaveChangesInterceptor>(failing)
        );
        await using var _host = host;
        failing.Fail = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            host.Get<RegistrationService>()
                .RegisterUser(
                    new RegisterMemberRequest { Token = code, UserInfo = NewMember("kofi@echo.test") },
                    Ct
                )
        );

        Assert.Equal(0, (await InvitationNow(host, invitationId)).UseCount);
    }

    [Fact]
    public async Task RegisterUser_WhenTheEmailIsTakenAfterTheCheck_Returns400_AndKeepsTheInvitationUse()
    {
        var race = new BeforeNextSaveInterceptor();
        var (host, code, invitationId, congregationId) = await Arrange(s =>
            s.AddSingleton<ISaveChangesInterceptor>(race)
        );
        await using var _host = host;
        // Someone else registers the same email between our "is it taken?" check and our insert.
        race.Arm(async () =>
        {
            using var other = host.NewScope();
            other.ServiceProvider.GetRequiredService<UserRepository>()
                .Create(TestHostExtensions.NewUser(congregationId, "kofi@echo.test"));
            await other.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync(Ct);
        });

        var result = await host.Get<RegistrationService>()
            .RegisterUser(
                new RegisterMemberRequest { Token = code, UserInfo = NewMember("kofi@echo.test") },
                Ct
            );

        Assert.IsType<BadRequestResult>(result);
        Assert.Equal("Email already in use", Detail(result));
        Assert.Equal(0, (await InvitationNow(host, invitationId)).UseCount);
    }

    [Fact]
    public async Task RegisterUser_LastUseOfLimitedInvitation_OnlyOneOfTwoConcurrentRequestsSucceeds()
    {
        var (host, code, invitationId, _) = await Arrange(_ => { }, maxUses: 1);
        await using var _host = host;

        async Task<IOperationResult> Register(string email)
        {
            using var scope = host.NewScope();
            return await scope
                .ServiceProvider.GetRequiredService<RegistrationService>()
                .RegisterUser(
                    new RegisterMemberRequest { Token = code, UserInfo = NewMember(email) },
                    Ct
                );
        }

        var results = await Task.WhenAll(Register("a@echo.test"), Register("b@echo.test"));

        Assert.Equal(1, results.Count(r => r is not BadRequestResult));
        Assert.Equal(1, results.Count(r => r is BadRequestResult));
        Assert.Equal(1, (await InvitationNow(host, invitationId)).UseCount);
    }
}
