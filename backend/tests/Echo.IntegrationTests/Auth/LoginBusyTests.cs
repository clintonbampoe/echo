using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class LoginBusyTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class BusyHasher : IPasswordHasher
    {
        public Task<string> HashAsync(string password, CancellationToken ct = default) =>
            Task.FromResult("hash");

        public Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default) =>
            throw new HashingBusyException(TimeSpan.FromSeconds(4));
    }

    [Theory]
    [InlineData("known@echo.test")]
    [InlineData("nobody@echo.test")]
    public async Task Login_WhenPasswordChecksAreBacklogged_IsAskedToWait_ForAKnownAndAnUnknownEmailAlike(
        string email
    )
    {
        await using var host = await AuthTestHost.CreateAsync(
            db,
            services => services.AddSingleton<IPasswordHasher>(new BusyHasher())
        );
        var congregationId = await host.SeedCongregationAsync();
        host.Get<UserRepository>().Create(TestHostExtensions.NewUser(congregationId, "known@echo.test"));
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var result = await host.Get<SessionService>().Login(email, "whatever", Ct);

        Assert.IsType<ServiceBusyResult>(result);
    }
}
