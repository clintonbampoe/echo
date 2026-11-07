using Echo.Shared.Services.Hashing;

namespace Echo.Shared.Tests.Services.Hashing;

[Trait("Category", "Unit")]
public class HashGateTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly TimeSpan _soon = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    [InlineData(8, 7)]
    public void DefaultPermits_IsOneLessThanTheCores_ButNeverZero(int cores, int expected) =>
        Assert.Equal(expected, HashGate.DefaultPermits(cores));

    [Fact]
    public async Task OnlyAsManyAsThePermitsRunAtOnce_TheRestWait()
    {
        using var gate = new HashGate(permits: 2, maxQueued: 10);
        var first = await gate.EnterAsync(Ct);
        using var second = await gate.EnterAsync(Ct);

        var third = gate.EnterAsync(Ct);

        Assert.False(third.IsCompleted);
        Assert.Equal(2, gate.InUse);
        Assert.Equal(1, gate.Waiting);

        first.Dispose();
        using var thirdPermit = await third.WaitAsync(_soon, Ct);

        Assert.Equal(2, gate.InUse);
        Assert.Equal(0, gate.Waiting);
    }

    [Fact]
    public async Task WhenTheQueueIsFull_TheNextCallerIsRefusedAtOnce()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 2);
        using var running = await gate.EnterAsync(Ct);
        _ = gate.EnterAsync(Ct);
        _ = gate.EnterAsync(Ct);

        var busy = await Assert.ThrowsAsync<HashingBusyException>(() => gate.EnterAsync(Ct));

        Assert.True(busy.RetryAfter >= TimeSpan.FromSeconds(1));
        Assert.Equal(2, gate.Waiting); // the refused caller never joined
    }

    [Fact]
    public async Task ACallerWhoGivesUp_LeavesTheQueue_AndLeaksNoPermit()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        var running = await gate.EnterAsync(Ct);
        using var giveUp = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        var waiting = gate.EnterAsync(giveUp.Token);

        giveUp.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        Assert.Equal(0, gate.Waiting);

        running.Dispose();
        using var next = await gate.EnterAsync(Ct).WaitAsync(_soon, Ct);
        Assert.Equal(1, gate.InUse);
    }

    [Fact]
    public async Task WaitersAreServedInTheOrderTheyArrived()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        var running = await gate.EnterAsync(Ct);
        var first = gate.EnterAsync(Ct);
        var second = gate.EnterAsync(Ct);

        running.Dispose();
        var firstPermit = await first.WaitAsync(_soon, Ct);

        Assert.False(second.IsCompleted);

        firstPermit.Dispose();
        using var secondPermit = await second.WaitAsync(_soon, Ct);
    }
}
