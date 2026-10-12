using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public static partial class SessionLabelLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label list: {Count} labels returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label search: {Count} results for query '{Query}' in congregation {CongregationId}"
    )]
    public static partial void Searched(
        ILogger logger,
        Guid congregationId,
        string query,
        int count
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label found: {LabelId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session label not found: {LabelId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label created: {LabelId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label updated: {LabelId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session label update failed: {LabelId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Session label deleted: {LabelId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Session label delete failed: {LabelId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, int labelId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Session label error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
