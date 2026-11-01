using Echo.Shared.Services.Hashing;

namespace Echo.Shared.Tests.Services.Hashing;

[Trait("Category", "Unit")]
public class BcryptHashServiceTests
{
    private readonly BcryptHashService _hasher = new();

    [Fact]
    public async Task HashThenVerify_RoundTrips()
    {
        var hash = await _hasher.HashAsync("correct horse battery");

        Assert.True(await _hasher.VerifyAsync("correct horse battery", hash));
    }

    [Fact]
    public async Task Hash_UsesCostFactorTen()
    {
        var hash = await _hasher.HashAsync("correct horse battery");

        Assert.Matches(@"^\$2[aby]\$10\$", hash);
    }

    [Fact]
    public async Task Verify_WrongPassword_False()
    {
        var hash = await _hasher.HashAsync("correct horse battery");

        Assert.False(await _hasher.VerifyAsync("wrong horse battery", hash));
    }

    [Theory]
    [InlineData("Spin")]
    [InlineData("")]
    [InlineData("plaintext-password")]
    public async Task Verify_AHashThatIsNotBcrypt_IsFalse_NotAnException(string hash)
    {
        // A row written before passwords were hashed holds the plain password. Signing in to it
        // must be refused like any wrong password, not crash the request.
        Assert.False(await _hasher.VerifyAsync("Spin", hash));
    }

    [Fact]
    public void HashAsync_RunsOnTheCallingThread()
    {
        // Task.Run only moved the work to another threadpool thread: a request already runs on
        // one, so it added a hop and no concurrency. The returned task is finished on return.
        var task = _hasher.HashAsync("correct horse battery");

        Assert.True(task.IsCompleted);
    }
}
