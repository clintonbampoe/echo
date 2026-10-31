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

    [Fact]
    public void HashAsync_RunsOnTheCallingThread()
    {
        // Task.Run only moved the work to another threadpool thread: a request already runs on
        // one, so it added a hop and no concurrency. The returned task is finished on return.
        var task = _hasher.HashAsync("correct horse battery");

        Assert.True(task.IsCompleted);
    }
}
