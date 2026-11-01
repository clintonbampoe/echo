using Microsoft.Extensions.Logging;

namespace Echo.Application.Caching;

public static partial class SummaryLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Summary {Summary} computed for congregation {CongregationId}"
    )]
    public static partial void Computed(ILogger logger, string summary, Guid congregationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Summary {Summary} served for congregation {CongregationId} (cached: {Cached})"
    )]
    public static partial void Served(
        ILogger logger,
        string summary,
        Guid congregationId,
        bool cached
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Summary {Summary} refreshed for congregation {CongregationId}"
    )]
    public static partial void Refreshed(ILogger logger, string summary, Guid congregationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Summary {Summary} could not be refreshed for congregation {CongregationId}; the old answer stays until it expires"
    )]
    public static partial void RefreshFailed(
        ILogger logger,
        Exception exception,
        string summary,
        Guid congregationId
    );
}
