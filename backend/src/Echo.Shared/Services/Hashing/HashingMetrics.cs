using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Echo.Shared.Services.Hashing;

/// <summary>
/// A permit that also records how long the hash held it; dispose it when the hash is done.
/// </summary>
public readonly struct HashScope : IDisposable
{
    private readonly HashPermit _permit;
    private readonly HashingMetrics? _metrics;
    private readonly long _started;

    internal HashScope(HashPermit permit, HashingMetrics metrics)
    {
        _permit = permit;
        _metrics = metrics;
        _started = metrics.Timestamp();
    }

    public void Dispose()
    {
        _metrics?.RecordDuration(_started);
        _permit.Dispose();
    }
}

/// <summary>
/// Enters the <see cref="HashGate"/> and reports on it:
/// queue wait, hash time, refusals, abandoned waits.
/// </summary>
public sealed class HashingMetrics
{
    public const string MeterName = "Echo.Shared.Hashing";

    private readonly HashGate _gate;
    private readonly TimeProvider _time;
    private readonly Histogram<double> _wait;
    private readonly Histogram<double> _duration;
    private readonly Counter<long> _refused;
    private readonly Counter<long> _abandoned;

    public HashingMetrics(IMeterFactory meters, HashGate gate, TimeProvider time)
    {
        _gate = gate;
        _time = time;
        var meter = meters.Create(MeterName);

        meter.CreateObservableGauge(
            "auth.hash.in_use",
            () => gate.InUse,
            unit: "{hash}",
            description: "Password hashes running now"
        );
        meter.CreateObservableGauge(
            "auth.hash.waiting",
            () => gate.Waiting,
            unit: "{hash}",
            description: "Password hashes waiting for a free permit"
        );
        // Seconds, in steps that fit a hash of 0.05 s and a line of up to 10 s.
        // OpenTelemetry's default buckets are for milliseconds and would put
        // all of these in the first one.
        _wait = meter.CreateHistogram(
            "auth.hash.wait",
            unit: "s",
            description: "Time a password hash waited for a free permit",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    0.001,
                    0.01,
                    0.05,
                    0.1,
                    0.25,
                    0.5,
                    1,
                    2.5,
                    5,
                    7.5,
                    10,
                    15,
                    30,
                ],
            }
        );
        _duration = meter.CreateHistogram(
            "auth.hash.duration",
            unit: "s",
            description: "Time a password hash took once it started: the cost the queue is sized on",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    0.01,
                    0.02,
                    0.025,
                    0.03,
                    0.035,
                    0.04,
                    0.045,
                    0.05,
                    0.06,
                    0.075,
                    0.1,
                    0.15,
                    0.2,
                    0.3,
                    0.5,
                    1,
                    2.5,
                ],
            }
        );
        _refused = meter.CreateCounter<long>(
            "auth.hash.refused",
            unit: "{hash}",
            description: "Password hashes refused because too many were already waiting"
        );
        _abandoned = meter.CreateCounter<long>(
            "auth.hash.abandoned",
            unit: "{hash}",
            description: "Callers who gave up (closed the page, timed out) while waiting for a permit"
        );
    }

    public async Task<HashScope> EnterAsync(CancellationToken ct)
    {
        var started = _time.GetTimestamp();
        HashPermit permit;
        try
        {
            permit = await _gate.EnterAsync(ct).ConfigureAwait(false);
        }
        catch (HashingBusyException)
        {
            _refused.Add(1);
            throw;
        }
        catch (OperationCanceledException)
        {
            _abandoned.Add(1);
            throw;
        }

        var waited = _time.GetElapsedTime(started);
        _wait.Record(waited.TotalSeconds);
        // Time in line is expected under a burst and is not the request being slow.
        // Saying so on the request's own span
        // lets the slow-request log (Alloy) judge the work and leave the waiting out.
        ServerSpan()?.SetTag("auth.hash.wait_ms", (long)waited.TotalMilliseconds);
        return new HashScope(permit, this);
    }

    internal long Timestamp() => _time.GetTimestamp();

    internal void RecordDuration(long started) =>
        _duration.Record(_time.GetElapsedTime(started).TotalSeconds);

    // The span of the HTTP request: the hash runs inside child spans, and slow requests
    // are judged on the server span.
    private static Activity? ServerSpan()
    {
        for (var span = Activity.Current; span is not null; span = span.Parent)
            if (span.Kind == ActivityKind.Server)
                return span;
        return null;
    }
}
