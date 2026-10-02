using Microsoft.Extensions.Logging;

namespace Echo.Application.Organizations;

public static partial class OrganizationLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization list: {Count} organizations returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization search: {Count} results for query '{Query}' in congregation {CongregationId}"
    )]
    public static partial void Searched(
        ILogger logger,
        Guid congregationId,
        string query,
        int count
    );

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization found: {OrganizationId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization not found: {OrganizationId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid organizationId);

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization created: {OrganizationId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization updated: {OrganizationId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization update failed: {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization deleted: {OrganizationId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization delete failed: {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Organization error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
