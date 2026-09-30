using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public static partial class ProjectContributionLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution created: {ContributionId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution updated: {ContributionId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution deleted: {ContributionId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution not found: {ContributionId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution creation failed: Project {ProjectId} not found"
    )]
    public static partial void ProjectNotFound(ILogger logger, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Project contribution error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
