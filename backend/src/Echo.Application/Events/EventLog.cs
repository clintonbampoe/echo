using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event created: {EventId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event updated: {EventId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid eventId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Event deleted: {EventId} soft-deleted")]
    public static partial void Deleted(ILogger logger, Guid eventId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Event not found: {EventId} requested")]
    public static partial void NotFound(ILogger logger, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event creation failed: Organizer {MemberId} not found"
    )]
    public static partial void OrganizerNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event creation failed: Organization {OrganizationId} not found"
    )]
    public static partial void OrganizationNotFound(ILogger logger, Guid organizationId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Event error: unexpected failure - {Message}")]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
