using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public static partial class ProjectContributionLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution list: {Count} contributions returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution found: {ContributionId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution not found: {ContributionId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid contributionId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution create: project {ProjectId} found in congregation {CongregationId}"
    )]
    public static partial void CreateProjectFound(
        ILogger logger,
        Guid congregationId,
        Guid projectId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution create failed: project {ProjectId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateProjectNotFound(
        ILogger logger,
        Guid congregationId,
        Guid projectId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution update: project {ProjectId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateProjectFound(
        ILogger logger,
        Guid congregationId,
        Guid projectId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution update failed: project {ProjectId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateProjectNotFound(
        ILogger logger,
        Guid congregationId,
        Guid projectId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution created: {ContributionId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution updated: {ContributionId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution update failed: {ContributionId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid contributionId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project contribution deleted: {ContributionId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid contributionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project contribution delete failed: {ContributionId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid contributionId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Project contribution error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
