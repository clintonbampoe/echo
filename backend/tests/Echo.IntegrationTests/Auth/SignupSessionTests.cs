using System.IdentityModel.Tokens.Jwt;
using Echo.Application.Congregations;
using Echo.Application.Users;
using Echo.Auth.Invitations;
using Echo.Auth.Registrations;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class SignupSessionTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static UserCreateDto NewMember(string email = "kofi@echo.test") =>
        new()
        {
            FirstName = "Kofi",
            LastName = "Boateng",
            EmailAddress = email,
            Password = "correct horse battery",
        };

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

    private async Task<(AuthTestHost Host, string InvitationCode, Guid CongregationId)> Arrange()
    {
        var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(new RecordingEmailQueue())
        );
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
        return (host, invitation.Value<InviteResponseDto>().Token, congregationId);
    }

    [Fact]
    public async Task RegisterUser_ReturnsATokenPair_AndTheRefreshTokenWorks()
    {
        var (host, code, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct);

        var pair = result.Value<TokenPairResponseDtos>();
        Assert.False(string.IsNullOrEmpty(pair.AccessToken));
        var refreshed = await host.Get<SessionService>().RefreshAccessToken(pair.RefreshToken, Ct);
        Assert.False(string.IsNullOrEmpty(refreshed.Value<TokenPairResponseDtos>().AccessToken));
    }

    [Fact]
    public async Task RegisterCongregation_ReturnsATokenPair_ForTheNewAdmin()
    {
        await using var host = await AuthTestHost.CreateAsync(
            db,
            TestHostExtensions.WithRecordedEmail(new RecordingEmailQueue())
        );

        var result = await host.Get<RegistrationService>()
            .RegisterCongregation(NewCongregation(), NewMember("admin@echo.test"), Ct);

        var pair = result.Value<TokenPairResponseDtos>();
        var admin = await host.Get<UserRepository>().GetByEmail("admin@echo.test", Ct);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pair.AccessToken);
        Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == "role").Value);
        Assert.Equal(admin!.CongregationId.ToString(), jwt.Claims.Single(c => c.Type == "congregationId").Value);
        Assert.Equal(admin.Id.ToString(), jwt.Subject);
    }

    [Fact]
    public async Task RegisterUser_EmailTaken_ReturnsNoSession_AndStoresNoRefreshToken()
    {
        var (host, code, congregationId) = await Arrange();
        await using var _host = host;
        host.Get<UserRepository>().Create(TestHostExtensions.NewUser(congregationId, "kofi@echo.test"));
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var result = await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct);

        Assert.IsType<BadRequestResult>(result);
        Assert.Equal(0, await host.Get<AppDbContext>().Set<RefreshToken>().CountAsync(Ct));
    }

    [Fact]
    public async Task Signup_SavesUserAndSessionInOneCommit()
    {
        var (host, code, _) = await Arrange();
        await using var _host = host;
        host.Commands.Reset();

        await host.Get<RegistrationService>()
            .RegisterUser(new RegisterMemberRequest { Token = code, UserInfo = NewMember() }, Ct);

        // Same count as RegistrationEmailTests: the refresh token and the verification token ride
        // in the user's insert, and a brand-new user has nothing to prune.
        Assert.Equal(4, host.Commands.Count);
    }
}
