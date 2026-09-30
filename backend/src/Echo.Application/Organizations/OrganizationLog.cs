using Microsoft.Extensions.Logging;

namespace Echo.Application.Organizations;

public static partial class OrganizationLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization created: {OrganizationId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization updated: {OrganizationId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization deleted: {OrganizationId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization not found: {OrganizationId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Organization error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
