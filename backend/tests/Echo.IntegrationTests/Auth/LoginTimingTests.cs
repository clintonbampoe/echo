using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.IntegrationTests.Support;
using Echo.Shared.Services.Hashing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class LoginTimingTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class CountingHasher : IPasswordHasher
    {
        public int Verifications { get; private set; }

        public Task<string> HashAsync(string password, CancellationToken ct = default) =>
            Task.FromResult("hash");

        public Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default)
        {
            Verifications++;
            return Task.FromResult(false);
        }
    }

    private static string Detail(Echo.Shared.HttpResults.IOperationResult result) =>
        ((ProblemHttpResult)result.ToResult()).ProblemDetails.Detail!;

    [Fact]
    public async Task Login_UnknownEmail_StillRunsOnePasswordVerification_AndReturnsTheSameError()
    {
        var hasher = new CountingHasher();
        await using var host = await AuthTestHost.CreateAsync(
            db,
            services => services.AddSingleton<IPasswordHasher>(hasher)
        );
        var congregationId = await host.SeedCongregationAsync();
        host.Get<UserRepository>().Create(TestHostExtensions.NewUser(congregationId, "known@echo.test"));
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        var sessions = host.Get<SessionService>();

        var wrongPassword = await sessions.Login("known@echo.test", "nope", Ct);
        var verificationsForKnown = hasher.Verifications;
        var unknownEmail = await sessions.Login("nobody@echo.test", "nope", Ct);

        Assert.Equal(1, verificationsForKnown);
        Assert.Equal(2, hasher.Verifications); // one more for the unknown email, not zero
        Assert.Equal(Detail(wrongPassword), Detail(unknownEmail));
    }
}
