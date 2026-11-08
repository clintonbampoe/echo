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

    public Counter<long> CongregationRequestVolume { get; }

    /// <summary>Cacheable reads, by route and whether the answer came from the cache.</summary>
    public Counter<long> CacheRequests { get; }

    public ApplicationInstrumentation()
    {
        CongregationRequestVolume = Meter.CreateCounter<long>(
            "echo.congregation.request_volume",
            unit: "{request}",
            description: "Total requests per congregation"
        );

        CacheRequests = Meter.CreateCounter<long>(
            "echo.cache.requests",
            unit: "{request}",
            description: "Cacheable reads, by route and whether the answer was cached"
        );
    }

    public static TracerProviderBuilder ConfigureTracing(TracerProviderBuilder tracing) =>
        tracing.AddSource(SourceName);

    public static MeterProviderBuilder ConfigureMetrics(MeterProviderBuilder metrics) =>
        metrics.AddMeter(SourceName);

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}
