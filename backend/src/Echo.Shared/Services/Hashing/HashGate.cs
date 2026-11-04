using System.Diagnostics;
using System.Diagnostics.Metrics;

using Echo.Shared.Options.Hashing;

namespace Echo.Shared.Services.Hashing;

/// <summary>Held while a password is being hashed or checked; dispose it to let the next one in.</summary>
public readonly struct HashPermit : IDisposable
{
    private readonly HashGate? _gate;
    private readonly long _started;

    internal HashPermit(HashGate gate)
    {
        _gate = gate;
        _started = Stopwatch.GetTimestamp();
    }

    public void Dispose() => _gate?.Release(_started);
}

/// <summary>
/// Limits how many password hashes run at once. A bcrypt hash is about 175 ms of CPU on a thread of its own, so a crowd
/// signing in together would take every core and every pool thread from the people who are already signed in. With
/// <c>permits</c> hashes running, the rest wait in line without holding a thread, and when the line is
/// longer than <c>maxQueued</c> the next caller is refused at once with a wait estimate instead of joining a queue it
/// would give up on.
/// </summary>
public sealed class HashGate : IDisposable
{
    public const string MeterName = "Echo.Shared.Hashing";

    // What one bcrypt hash costs at cost factor 10, to turn a queue length into a wait for the caller to be told.
    internal const double ExpectedHashSeconds = 0.2;

    private readonly SemaphoreSlim _permits;
    private readonly int _permitCount;
    private readonly int _maxQueued;
    private readonly Meter _meter = new(MeterName);
    private readonly Histogram<double> _wait;
    private readonly Histogram<double> _duration;
    private readonly Counter<long> _refused;
    private readonly Counter<long> _abandoned;
    private int _waiting;

    public HashGate(int permits, int maxQueued)
    {
        _permitCount = permits;
        _maxQueued = maxQueued;
        _permits = new SemaphoreSlim(permits, permits);

        _meter.CreateObservableGauge(
            "auth.hash.in_use",
            () => InUse,
            unit: "{hash}",
            description: "Password hashes running now"
        );
        _meter.CreateObservableGauge(
            "auth.hash.waiting",
            () => Waiting,
            unit: "{hash}",
            description: "Password hashes waiting for a free permit"
        );
        // Seconds, in steps that fit a hash of 0.2 s and a line of up to 10 s. OpenTelemetry's default buckets are for
        // milliseconds and would put all of these in the first one.
        _wait = _meter.CreateHistogram<double>(
            "auth.hash.wait",
            unit: "s",
            description: "Time a password hash waited for a free permit",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [0.001, 0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 7.5, 10, 15, 30],
            }
        );
        _duration = _meter.CreateHistogram<double>(
            "auth.hash.duration",
            unit: "s",
            description: "Time a password hash took once it started: the cost the queue is sized on (0.2 s)",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [0.05, 0.1, 0.125, 0.15, 0.175, 0.2, 0.225, 0.25, 0.3, 0.4, 0.5, 0.75, 1, 2.5],
            }
        );
        _refused = _meter.CreateCounter<long>(
            "auth.hash.refused",
            unit: "{hash}",
            description: "Password hashes refused because too many were already waiting"
        );
        _abandoned = _meter.CreateCounter<long>(
            "auth.hash.abandoned",
            unit: "{hash}",
            description: "Callers who gave up (closed the page, timed out) while waiting for a permit"
        );
    }

    public int Permits => _permitCount;
    public int MaxQueued => _maxQueued;

    public static HashGate From(HashingOptions options, int processorCount)
    {
        var permits = options.MaxConcurrent ?? DefaultPermits(processorCount);
        return new HashGate(permits, (int)(permits * options.MaxQueueSeconds / ExpectedHashSeconds));
    }

    /// <summary>One core is left for everything else the API does.</summary>
    public static int DefaultPermits(int processorCount) => Math.Max(1, processorCount - 1);

    public int InUse => _permitCount - _permits.CurrentCount;
    public int Waiting => Volatile.Read(ref _waiting);

    public Task<HashPermit> EnterAsync(CancellationToken ct)
    {
        // Already finished when a permit is free, so an unloaded API pays nothing for the gate.
        if (_permits.Wait(0))
        {
            _wait.Record(0);
            return Task.FromResult(new HashPermit(this));
        }

        return EnterQueuedAsync(ct);
    }

    private async Task<HashPermit> EnterQueuedAsync(CancellationToken ct)
    {
        var waiting = Interlocked.Increment(ref _waiting);
        if (waiting > _maxQueued)
        {
            Interlocked.Decrement(ref _waiting);
            _refused.Add(1);
            throw new HashingBusyException(RetryAfter(waiting));
        }

        var server = ServerSpan();
        var started = Stopwatch.GetTimestamp();
        try
        {
            await _permits.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _abandoned.Add(1);
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref _waiting);
        }

        var waited = Stopwatch.GetElapsedTime(started);
        _wait.Record(waited.TotalSeconds);
        // Time in line is expected under a burst and is not the request being slow. Saying so on the request's own span
        // lets the slow-request log (Alloy) judge the work and leave the waiting out.
        server?.SetTag("auth.hash.wait_ms", (long)waited.TotalMilliseconds);
        return new HashPermit(this);
    }

    // The span of the HTTP request: the hash runs inside child spans, and slow requests are judged on the server span.
    private static Activity? ServerSpan()
    {
        for (var span = Activity.Current; span is not null; span = span.Parent)
            if (span.Kind == ActivityKind.Server)
                return span;
        return null;
    }

    internal void Release(long started)
    {
        _duration.Record(Stopwatch.GetElapsedTime(started).TotalSeconds);
        _permits.Release();
    }

    private TimeSpan RetryAfter(int queued) =>
        TimeSpan.FromSeconds(
            Math.Max(1, Math.Ceiling(queued / (double)_permitCount * ExpectedHashSeconds))
        );

    public void Dispose()
    {
        _permits.Dispose();
        _meter.Dispose();
    }
}
