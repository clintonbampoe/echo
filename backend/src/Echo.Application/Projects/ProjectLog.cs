using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public static partial class ProjectLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project list: {Count} projects returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Project found: {ProjectId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project not found: {ProjectId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid projectId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project create: manager {ManagerId} found in congregation {CongregationId}"
    )]
    public static partial void CreateManagerFound(
        ILogger logger,
        Guid congregationId,
        Guid managerId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project create failed: manager {ManagerId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateManagerNotFound(
        ILogger logger,
        Guid congregationId,
        Guid managerId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project create: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project create failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project update: manager {ManagerId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateManagerFound(
        ILogger logger,
        Guid congregationId,
        Guid managerId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project update failed: manager {ManagerId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateManagerNotFound(
        ILogger logger,
        Guid congregationId,
        Guid managerId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project update: category {CategoryId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project update failed: category {CategoryId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateCategoryNotFound(
        ILogger logger,
        Guid congregationId,
        int categoryId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project created: {ProjectId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project updated: {ProjectId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project update failed: {ProjectId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project deleted: {ProjectId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project delete failed: {ProjectId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid projectId);

    // --- Summary ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project summary: {TotalProjects} projects summarized for congregation {CongregationId}"
    )]
    public static partial void Summarized(ILogger logger, Guid congregationId, int totalProjects);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Project error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
