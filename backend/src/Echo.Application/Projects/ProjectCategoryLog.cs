using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public static partial class ProjectCategoryLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project category created: {ProjectCategoryId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, int projectCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project category updated: {ProjectCategoryId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, int projectCategoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Project category deleted: {ProjectCategoryId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, int projectCategoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Project category not found: {ProjectCategoryId} requested"
    )]
    public static partial void NotFound(ILogger logger, int projectCategoryId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Project category error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
