using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventRegistrationLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration created: {RegistrationId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration updated: {RegistrationId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration deleted: {RegistrationId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration not found: {RegistrationId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration creation failed: Event {EventId} not found"
    )]
    public static partial void EventNotFound(ILogger logger, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration creation failed: Member {MemberId} not found"
    )]
    public static partial void MemberNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Event registration error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
