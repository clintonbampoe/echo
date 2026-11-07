using System.Diagnostics.Metrics;

namespace Echo.Shared.Tests.Services.Hashing;

// Meters made here carry this factory as their scope, so a listener can
// tell them from the ones other tests create in parallel.
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _created = [];

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options.Name, options.Version, options.Tags, scope: this);
        lock (_created)
            _created.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _created)
            meter.Dispose();
    }
}
