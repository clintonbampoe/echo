using Echo.Shared.Options.Hashing;

namespace Echo.Shared.Services.Hashing;

/// <summary>
/// Held while a password is being hashed or checked; dispose it to let the
/// next one in.
/// </summary>
public readonly struct HashPermit : IDisposable
{
    private readonly HashGate? _gate;

    internal HashPermit(HashGate gate) => _gate = gate;

    public void Dispose() => _gate?.Release();
}

/// <summary>
/// Limits how many password hashes run at once. An Argon2id hash is about
/// 25-40 ms of CPU on a thread of its own, so a crowd signing in together
/// would take every core and every pool thread from the people who are
/// already signed in. With <c>permits</c> hashes running, the rest wait in
/// line without holding a thread, and when the line is longer than
/// <c>maxQueued</c> the next caller is refused at once with a wait estimate
/// instead of joining a queue it would give up on.
/// </summary>
public sealed class HashGate(int permits, int maxQueued) : IDisposable
{
    // What one Argon2id hash (19 MiB, two passes) costs, to turn a queue
    // length into a wait for the caller to be told. Measured at 25-30 ms on
    // an idle core; the rest is room for a busy one, so the CPU is not driven
    // flat out.
    internal const double ExpectedHashSeconds = 0.05;

    private readonly SemaphoreSlim _permits = new(permits, permits);
    private readonly int _maxQueued = maxQueued;
    private int _waiting;

    public int Permits { get; } = permits;
    public int MaxQueued => _maxQueued;
    public int InUse => Permits - _permits.CurrentCount;
    public int Waiting => Volatile.Read(ref _waiting);

    public static HashGate From(HashingOptions options, int processorCount)
    {
        var permits = options.MaxConcurrent ?? DefaultPermits(processorCount);
        return new HashGate(
            permits,
            (int)(permits * options.MaxQueueSeconds / ExpectedHashSeconds)
        );
    }

    /// <summary>One core is left for everything else the API does.</summary>
    public static int DefaultPermits(int processorCount) => Math.Max(1, processorCount - 1);

    public Task<HashPermit> EnterAsync(CancellationToken ct)
    {
        // Already finished when a permit is free, so an unloaded API pays
        // nothing for the gate.
        if (_permits.Wait(0, ct))
            return Task.FromResult(new HashPermit(this));

        return EnterQueuedAsync(ct);
    }

    private async Task<HashPermit> EnterQueuedAsync(CancellationToken ct)
    {
        var waiting = Interlocked.Increment(ref _waiting);
        if (waiting > _maxQueued)
        {
            Interlocked.Decrement(ref _waiting);
            throw new HashingBusyException(RetryAfter(waiting));
        }

        try
        {
            await _permits.WaitAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _waiting);
        }

        return new HashPermit(this);
    }

    internal void Release() => _permits.Release();

    private TimeSpan RetryAfter(int queued) =>
        TimeSpan.FromSeconds(
            Math.Max(1, Math.Ceiling(queued / (double)Permits * ExpectedHashSeconds))
        );

    public void Dispose() => _permits.Dispose();
}
