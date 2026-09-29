using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Echo.Application;

public sealed class ApplicationInstrumentation : IDisposable
{
    public const string SourceName = "Echo.Application";

    private static readonly string? _version = typeof(ApplicationInstrumentation)
        .Assembly.GetName()
        .Version?.ToString();

    public ActivitySource ActivitySource { get; } = new(SourceName, _version);
    public Meter Meter { get; } = new(SourceName, _version);

    // Extension method pattern for a cleaner, self-contained Program.cs configuration
    public static TracerProviderBuilder ConfigureTracing(TracerProviderBuilder tracing)
    {
        return tracing.AddSource(SourceName);
    }

    public static MeterProviderBuilder ConfigureMetrics(MeterProviderBuilder metrics)
    {
        return metrics.AddMeter(SourceName);
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}
