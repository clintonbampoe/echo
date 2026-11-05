using System.ComponentModel.DataAnnotations;
using Echo.Shared.Options.Hashing;
using Echo.Shared.Services.Hashing;

namespace Echo.Shared.Tests.Services.Hashing;

[Trait("Category", "Unit")]
// Same collection: these tests listen to the gate's meter, which every gate in the process shares.
[Collection("HashGate")]
public class HashGateTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly TimeSpan Soon = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    [InlineData(8, 7)]
    public void DefaultPermits_IsOneLessThanTheCores_ButNeverZero(int cores, int expected) =>
        Assert.Equal(expected, HashGate.DefaultPermits(cores));

    [Fact]
    public async Task OnlyAsManyAsThePermitsRunAtOnce_TheRestWaitUntilOneIsReleased()
    {
        using var gate = new HashGate(permits: 2, maxQueued: 10);
        var first = await gate.EnterAsync(Ct);
        using var second = await gate.EnterAsync(Ct);

        var third = gate.EnterAsync(Ct);

        Assert.False(third.IsCompleted);
        Assert.Equal(2, gate.InUse);
        Assert.Equal(1, gate.Waiting);

        first.Dispose();
        using var thirdPermit = await third.WaitAsync(Soon, Ct);

        Assert.Equal(2, gate.InUse);
        Assert.Equal(0, gate.Waiting);
    }

    [Fact]
    public async Task WhenTheQueueIsFull_TheNextCallerIsRefusedAtOnce_WithAWaitEstimate()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 2);
        using var running = await gate.EnterAsync(Ct);
        _ = gate.EnterAsync(Ct);
        _ = gate.EnterAsync(Ct);

        var busy = await Assert.ThrowsAsync<HashingBusyException>(() => gate.EnterAsync(Ct));

        Assert.True(busy.RetryAfter >= TimeSpan.FromSeconds(1));
        Assert.Equal(2, gate.Waiting); // the refused caller never joined the queue
    }

    [Fact]
    public async Task ACallerWhoGivesUpWhileWaiting_LeavesTheQueue_AndLeaksNoPermit()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        var running = await gate.EnterAsync(Ct);
        using var giveUp = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        var waiting = gate.EnterAsync(giveUp.Token);

        giveUp.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        Assert.Equal(0, gate.Waiting);

        running.Dispose();
        using var next = await gate.EnterAsync(Ct).WaitAsync(Soon, Ct);
        Assert.Equal(1, gate.InUse);
    }

    [Fact]
    public async Task ACallerWhoGivesUpWhileWaiting_IsCounted_AsSomeoneWeLost()
    {
        long abandoned = 0;
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == HashGate.MeterName && instrument.Name == "auth.hash.abandoned")
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref abandoned, value));
        listener.Start();
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        using var running = await gate.EnterAsync(Ct);
        using var giveUp = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        var waiting = gate.EnterAsync(giveUp.Token);

        giveUp.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);

        Assert.Equal(1, Interlocked.Read(ref abandoned));
    }

    [Fact]
    public async Task HowLongAPermitWasHeld_IsRecordedAsTheTimeTheHashTook()
    {
        var seconds = new List<double>();
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == HashGate.MeterName && instrument.Name == "auth.hash.duration")
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<double>((_, value, _, _) => seconds.Add(value));
        listener.Start();
        using var gate = new HashGate(permits: 1, maxQueued: 5);

        var permit = await gate.EnterAsync(Ct);
        await Task.Delay(60, Ct);
        permit.Dispose();

        var recorded = Assert.Single(seconds);
        Assert.InRange(recorded, 0.05, 1);
    }

    [Theory]
    [InlineData("auth.hash.duration")]
    [InlineData("auth.hash.wait")]
    public void TheHistograms_AreBucketedInSubSecondSteps_NotTheMillisecondDefaults(string name)
    {
        IReadOnlyList<double>? boundaries = null;
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (instrument, _) =>
        {
            if (instrument.Meter.Name == HashGate.MeterName && instrument.Name == name)
                boundaries = (instrument as System.Diagnostics.Metrics.Histogram<double>)
                    ?.Advice?.HistogramBucketBoundaries;
        };
        listener.Start();

        using var gate = new HashGate(permits: 1, maxQueued: 5);

        Assert.NotNull(boundaries);
        Assert.Contains(boundaries!, b => b > 0 && b < 0.1); // the millisecond defaults start at 5
    }

    [Fact]
    public void TheHashDurationBuckets_ResolveMillisecondsAroundTheCostTheLineIsSizedOn()
    {
        IReadOnlyList<double>? boundaries = null;
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (instrument, _) =>
        {
            if (instrument.Meter.Name == HashGate.MeterName && instrument.Name == "auth.hash.duration")
                boundaries = (instrument as System.Diagnostics.Metrics.Histogram<double>)
                    ?.Advice?.HistogramBucketBoundaries;
        };
        listener.Start();

        using var gate = new HashGate(permits: 1, maxQueued: 5);

        // A hash is 25-40 ms, so a bucket per 10 ms or coarser would put every one in the same bucket.
        Assert.Contains(0.025, boundaries!);
        Assert.Contains(0.03, boundaries!);
        Assert.Contains(0.035, boundaries!);
        Assert.Contains(0.045, boundaries!);
    }

    [Fact]
    public async Task ARequestThatWaitedInLine_SaysSoOnItsServerSpan_SoSlownessCanBeToldFromWaiting()
    {
        using var source = new System.Diagnostics.ActivitySource("test.request");
        using var listener = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = s => s.Name == "test.request",
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) =>
                System.Diagnostics.ActivitySamplingResult.AllData,
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        var running = await gate.EnterAsync(Ct);

        using var server = source.StartActivity("request", System.Diagnostics.ActivityKind.Server);
        using var inner = source.StartActivity("svc.session.login"); // the code that hashes runs in a child span
        var waiting = gate.EnterAsync(Ct);
        await Task.Delay(80, Ct);
        running.Dispose();
        (await waiting.WaitAsync(Soon, Ct)).Dispose();

        var waited = Assert.IsType<long>(server!.GetTagItem("auth.hash.wait_ms"));
        Assert.InRange(waited, 60, 2000);
    }

    [Fact]
    public async Task ARequestThatDidNotHaveToWait_AddsNoTag()
    {
        using var source = new System.Diagnostics.ActivitySource("test.request.quick");
        using var listener = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = s => s.Name == "test.request.quick",
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) =>
                System.Diagnostics.ActivitySamplingResult.AllData,
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        using var server = source.StartActivity("request", System.Diagnostics.ActivityKind.Server);

        (await gate.EnterAsync(Ct)).Dispose();

        Assert.Null(server!.GetTagItem("auth.hash.wait_ms"));
    }

    [Fact]
    public async Task WaitersAreServedInTheOrderTheyArrived()
    {
        using var gate = new HashGate(permits: 1, maxQueued: 5);
        var running = await gate.EnterAsync(Ct);
        var first = gate.EnterAsync(Ct);
        var second = gate.EnterAsync(Ct);

        running.Dispose();
        var firstPermit = await first.WaitAsync(Soon, Ct);

        Assert.False(second.IsCompleted);

        firstPermit.Dispose();
        using var secondPermit = await second.WaitAsync(Soon, Ct);
    }
}

[Trait("Category", "Unit")]
public class HashingOptionsTests
{
    [Fact]
    public void WithoutASetting_UsesOneLessThanTheCores_AndAQueueOfTenSecondsOfWork()
    {
        using var gate = HashGate.From(new HashingOptions(), processorCount: 4);

        Assert.Equal(3, gate.Permits);
        Assert.Equal(600, gate.MaxQueued); // 3 permits x 10 s / 0.05 s a hash
    }

    [Fact]
    public void MaxConcurrent_OverridesTheCores()
    {
        using var gate = HashGate.From(new HashingOptions { MaxConcurrent = 5 }, processorCount: 2);

        Assert.Equal(5, gate.Permits);
    }

    [Fact]
    public void MaxQueueSeconds_SetsHowLongTheLineMayGet()
    {
        using var gate = HashGate.From(
            new HashingOptions { MaxQueueSeconds = 4 },
            processorCount: 2
        );

        Assert.Equal(80, gate.MaxQueued); // 1 permit x 4 s / 0.05 s a hash
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1000, 10)]
    [InlineData(2, 0)]
    public void ValuesOutOfRange_AreNotValid(int maxConcurrent, int maxQueueSeconds)
    {
        var options = new HashingOptions
        {
            MaxConcurrent = maxConcurrent,
            MaxQueueSeconds = maxQueueSeconds,
        };

        Assert.False(
            Validator.TryValidateObject(
                options,
                new ValidationContext(options),
                validationResults: null,
                validateAllProperties: true
            )
        );
    }
}
