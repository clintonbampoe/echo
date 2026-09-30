using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public static partial class ProjectLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project created: {ProjectId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project updated: {ProjectId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project deleted: {ProjectId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid projectId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Project not found: {ProjectId} requested")]
    public static partial void NotFound(ILogger logger, Guid projectId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project creation failed: Manager {MemberId} not found"
    )]
    public static partial void ManagerNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project creation failed: Category {CategoryId} not found"
    )]
    public static partial void CategoryNotFound(ILogger logger, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Project error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
