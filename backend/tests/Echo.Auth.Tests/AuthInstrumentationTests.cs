using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Echo.Auth.Tests;

[Trait("Category", "Unit")]
public class AuthInstrumentationTests
{
    private sealed record Measurement(long Value, Dictionary<string, object?> Tags);

    private static List<Measurement> Listen(AuthInstrumentation instrumentation, Action act)
    {
        var seen = new List<Measurement>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == AuthInstrumentation.SourceName)
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>(
            (_, value, tags, _) =>
                seen.Add(new Measurement(value, tags.ToArray().ToDictionary(t => t.Key, t => t.Value)))
        );
        listener.Start();
        act();
        return seen;
    }

    [Fact]
    public void Record_SetsTheResultTagOnTheSpan_AndIncrementsTheCounterWithTheSameTags()
    {
        using var instrumentation = new AuthInstrumentation();
        using var activity = new Activity("test").Start();

        var seen = Listen(instrumentation, () =>
            instrumentation.Record(instrumentation.LoginAttempts, activity, "invalid_credentials", "cong-1"));

        var m = Assert.Single(seen);
        Assert.Equal(1, m.Value);
        Assert.Equal("invalid_credentials", m.Tags["auth.result"]);
        Assert.Equal("cong-1", m.Tags["congregation.id"]);
        Assert.Equal("invalid_credentials", activity.GetTagItem("auth.result"));
    }

    [Fact]
    public void Record_WithoutACongregation_LeavesTheCongregationTagOut()
    {
        using var instrumentation = new AuthInstrumentation();

        var seen = Listen(instrumentation, () =>
            instrumentation.Record(instrumentation.CongregationRegistrations, null, "email_taken"));

        var m = Assert.Single(seen);
        Assert.Equal("email_taken", m.Tags["auth.result"]);
        Assert.False(m.Tags.ContainsKey("congregation.id"));
    }

    [Fact]
    public void Record_WithNullSpan_StillIncrements()
    {
        using var instrumentation = new AuthInstrumentation();

        var seen = Listen(instrumentation, () =>
            instrumentation.Record(instrumentation.TokenRevocations, null, "success"));

        Assert.Single(seen);
    }
}
