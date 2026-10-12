using Microsoft.Extensions.Logging;

namespace Echo.Application.Congregations;

public static partial class CongregationProvisioningLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Provisioning session labels: {Count} queued for congregation {CongregationId}"
    )]
    public static partial void SessionLabelsQueued(ILogger logger, Guid congregationId, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Provisioning transaction categories: {Count} queued for congregation {CongregationId}"
    )]
    public static partial void TransactionCategoriesQueued(
        ILogger logger,
        Guid congregationId,
        int count
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Provisioning project categories: {Count} queued for congregation {CongregationId}"
    )]
    public static partial void ProjectCategoriesQueued(
        ILogger logger,
        Guid congregationId,
        int count
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Provisioning asset categories: {Count} queued for congregation {CongregationId}"
    )]
    public static partial void AssetCategoriesQueued(
        ILogger logger,
        Guid congregationId,
        int count
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Provisioning complete: {Total} default rows queued for congregation {CongregationId}"
    )]
    public static partial void Completed(ILogger logger, Guid congregationId, int total);
}
