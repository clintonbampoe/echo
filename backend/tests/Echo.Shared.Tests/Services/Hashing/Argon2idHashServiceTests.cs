using Echo.Shared.Services.Hashing;

namespace Echo.Shared.Tests.Services.Hashing;

[Trait("Category", "Unit")]
public sealed class Argon2idHashServiceTests : IDisposable
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly TestMeterFactory _meters = new();
    private readonly HashGate _gate = new(permits: 2, maxQueued: 10);
    private readonly Argon2idHashService _hasher;

    public Argon2idHashServiceTests() =>
        _hasher = new Argon2idHashService(new HashingMetrics(_meters, _gate, TimeProvider.System));

    public void Dispose()
    {
        _gate.Dispose();
        _meters.Dispose();
    }

    [Fact]
    public async Task HashThenVerify_RoundTrips()
    {
        var hash = await _hasher.HashAsync("correct horse battery", Ct);

        Assert.True(await _hasher.VerifyAsync("correct horse battery", hash, Ct));
    }

    [Fact]
    public async Task Hash_UsesTwoPassesOf19MiB()
    {
        var hash = await _hasher.HashAsync("correct horse battery", Ct);

        Assert.Matches(@"^\$argon2id\$v=19\$m=19456,t=2,p=1\$", hash);
    }

    [Fact]
    public async Task Hash_OfTheSamePassword_DiffersEveryTime()
    {
        var first = await _hasher.HashAsync("correct horse battery", Ct);
        var second = await _hasher.HashAsync("correct horse battery", Ct);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Verify_WrongPassword_False()
    {
        var hash = await _hasher.HashAsync("correct horse battery", Ct);

        Assert.False(await _hasher.VerifyAsync("wrong horse battery", hash, Ct));
    }

    [Fact]
    public async Task HashThenVerify_RoundTrips_ForAPasswordThatIsNotAscii()
    {
        const string password = "pässwörd 密码 🔑";
        var hash = await _hasher.HashAsync(password, Ct);

        Assert.True(await _hasher.VerifyAsync(password, hash, Ct));
        Assert.False(await _hasher.VerifyAsync("passwoerd", hash, Ct));
    }

    [Theory]
    [InlineData("Spin")]
    [InlineData("")]
    [InlineData("plaintext-password")]
    [InlineData("$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")] // a bcrypt hash
    public async Task Verify_AHashThatIsNotArgon2id_IsFalse_NotAnException(string hash)
    {
        // A row written before passwords were hashed holds the plain password, and one from before Argon2id holds a
        // bcrypt hash. Signing in to either must be refused like any wrong password, not crash the request.
        Assert.False(await _hasher.VerifyAsync("Spin", hash, Ct));
    }

    [Fact]
    public async Task HashAsync_RunsOnTheCallingThread()
    {
        // A request already runs on a threadpool thread, so Task.Run would only add a hop. The returned task is
        // finished on return.
        var task = _hasher.HashAsync("correct horse battery", Ct);

        Assert.True(task.IsCompleted);
        await task;
    }

    [Fact]
    public async Task Verify_GivesItsPermitBack_WhateverTheAnswer()
    {
        var hash = await _hasher.HashAsync("correct horse battery", Ct);

        await _hasher.VerifyAsync("correct horse battery", hash, Ct);
        await _hasher.VerifyAsync("wrong horse battery", hash, Ct);
        await _hasher.VerifyAsync("Spin", "plaintext-password", Ct); // not an Argon2id hash

        Assert.Equal(0, _gate.InUse);
    }

    [Fact]
    public async Task Verify_WhenEveryPermitIsTakenAndNoOneMayWait_IsRefusedWithoutHashing()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 0);
        using var meters = new TestMeterFactory();
        var hasher = new Argon2idHashService(new HashingMetrics(meters, gate, TimeProvider.System));
        var hash = await _hasher.HashAsync("correct horse battery", Ct);
        using var taken = await gate.EnterAsync(Ct);

        await Assert.ThrowsAsync<HashingBusyException>(() =>
            hasher.VerifyAsync("correct horse battery", hash, Ct)
        );
        await Assert.ThrowsAsync<HashingBusyException>(() =>
            hasher.HashAsync("correct horse battery", Ct)
        );
    }
}
