using System.Diagnostics;
using System.Diagnostics.Metrics;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Shared.Tests.Services.Hashing;

[Trait("Category", "Unit")]
public sealed class HashingMetricsTests : IDisposable
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly TimeSpan _soon = TimeSpan.FromSeconds(5);

    private readonly TestMeterFactory _meters = new();
    private readonly HashGate _gate = new(permits: 1, maxQueued: 2);
    private readonly FakeTimeProvider _time = new();
    private readonly HashingMetrics _metrics;

    public HashingMetricsTests() => _metrics = new(_meters, _gate, _time);

    public void Dispose()
    {
        _gate.Dispose();
        _meters.Dispose();
    }

    [Fact]
    public async Task ACallerWhoGivesUpWhileWaiting_IsCountedAsAbandoned()
    {
        using var abandoned = new Recorder<long>(_meters, "auth.hash.abandoned");
        using var running = await _metrics.EnterAsync(Ct);
        using var giveUp = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        var waiting = _metrics.EnterAsync(giveUp.Token);

        giveUp.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);

        Assert.Equal(1, abandoned.Values.Sum());
    }

    [Fact]
    public async Task ACallerRefusedBecauseTheQueueIsFull_IsCountedAsRefused()
    {
        using var refused = new Recorder<long>(_meters, "auth.hash.refused");
        using var running = await _metrics.EnterAsync(Ct);
        _ = _metrics.EnterAsync(Ct);
        _ = _metrics.EnterAsync(Ct);

        await Assert.ThrowsAsync<HashingBusyException>(() => _metrics.EnterAsync(Ct));

        Assert.Equal(1, refused.Values.Sum());
    }

    [Fact]
    public async Task HowLongAScopeWasHeld_IsRecordedAsTheTimeTheHashTook()
    {
        using var duration = new Recorder<double>(_meters, "auth.hash.duration");

        var scope = await _metrics.EnterAsync(Ct);
        _time.Advance(TimeSpan.FromMilliseconds(60));
        scope.Dispose();

        Assert.Equal(0.06, Assert.Single(duration.Values), precision: 6);
    }

    [Fact]
    public async Task TimeSpentInLine_IsRecordedAsTheWait()
    {
        using var wait = new Recorder<double>(_meters, "auth.hash.wait");
        var running = await _metrics.EnterAsync(Ct);

        var queued = _metrics.EnterAsync(Ct);
        _time.Advance(TimeSpan.FromMilliseconds(60));
        running.Dispose();
        (await queued.WaitAsync(_soon, Ct)).Dispose();

        // the first caller did not wait; the second waited 60 ms
        Assert.Equal(2, wait.Values.Count);
        Assert.Equal(0, wait.Values.Min());
        Assert.Equal(0.06, wait.Values.Max(), precision: 6);
    }

    [Fact]
    public async Task TheGauges_ReportWhatTheGateIsDoingNow()
    {
        using var inUse = new Recorder<int>(_meters, "auth.hash.in_use");
        using var waiting = new Recorder<int>(_meters, "auth.hash.waiting");
        using var running = await _metrics.EnterAsync(Ct);
        _ = _metrics.EnterAsync(Ct);

        inUse.Observe();
        waiting.Observe();

        Assert.Equal(1, Assert.Single(inUse.Values));
        Assert.Equal(1, Assert.Single(waiting.Values));
    }

    [Theory]
    [InlineData("auth.hash.duration")]
    [InlineData("auth.hash.wait")]
    public void TheHistograms_AreBucketedInSubSecondSteps(string name)
    {
        var boundaries = Boundaries(_meters, name);

        Assert.NotNull(boundaries);
        // the millisecond defaults start at 5
        Assert.Contains(boundaries, b => b > 0 && b < 0.1);
    }

    [Fact]
    public void TheDurationBuckets_ResolveMillisecondsAroundTheSizingCost()
    {
        var boundaries = Boundaries(_meters, "auth.hash.duration");

        // A hash is 25-40 ms, so a bucket per 10 ms or coarser would put
        // every one in the same bucket.
        Assert.Contains(0.025, boundaries!);
        Assert.Contains(0.03, boundaries!);
        Assert.Contains(0.035, boundaries!);
        Assert.Contains(0.045, boundaries!);
    }

    [Fact]
    public async Task ARequestThatWaited_SaysSoOnItsServerSpan()
    {
        using var source = new ActivitySource("test.request");
        using var listener = SampleEverything("test.request");
        var running = await _metrics.EnterAsync(Ct);

        using var server = source.StartActivity("request", ActivityKind.Server);
        // the code that hashes runs in a child span
        using var inner = source.StartActivity("svc.session.login");
        var queued = _metrics.EnterAsync(Ct);
        _time.Advance(TimeSpan.FromMilliseconds(80));
        running.Dispose();
        (await queued.WaitAsync(_soon, Ct)).Dispose();

        var waited = Assert.IsType<long>(server!.GetTagItem("auth.hash.wait_ms"));
        Assert.Equal(80, waited);
    }

    [Fact]
    public async Task ARequestThatDidNotWait_TagsAWaitOfAboutZero()
    {
        using var source = new ActivitySource("test.request.quick");
        using var listener = SampleEverything("test.request.quick");
        using var server = source.StartActivity("request", ActivityKind.Server);

        (await _metrics.EnterAsync(Ct)).Dispose();

        var waited = Assert.IsType<long>(server!.GetTagItem("auth.hash.wait_ms"));
        Assert.Equal(0, waited);
    }

    private static ActivityListener SampleEverything(string sourceName)
    {
        static ActivitySamplingResult value(ref ActivityCreationOptions<ActivityContext> _) =>
            ActivitySamplingResult.AllData;
        var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == sourceName,
            Sample = value,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static IReadOnlyList<double>? Boundaries(object scope, string name)
    {
        IReadOnlyList<double>? found = null;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, _) =>
        {
            if (ReferenceEquals(instrument.Meter.Scope, scope) && instrument.Name == name)
                found = (instrument as Histogram<double>)?.Advice?.HistogramBucketBoundaries;
        };
        listener.Start();
        return found;
    }

    private sealed class Recorder<T> : IDisposable
        where T : struct
    {
        private readonly MeterListener _listener = new();
        private readonly List<T> _values = [];

        public Recorder(object scope, string name)
        {
            _listener.InstrumentPublished = (instrument, l) =>
            {
                if (ReferenceEquals(instrument.Meter.Scope, scope) && instrument.Name == name)
                    l.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<T>(
                (_, value, _, _) =>
                {
                    lock (_values)
                        _values.Add(value);
                }
            );
            _listener.Start();
        }

        public IReadOnlyList<T> Values
        {
            get
            {
                lock (_values)
                    return [.. _values];
            }
        }

        public void Observe() => _listener.RecordObservableInstruments();

        public void Dispose() => _listener.Dispose();
    }
}
