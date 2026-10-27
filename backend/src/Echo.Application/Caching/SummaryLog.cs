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
}
