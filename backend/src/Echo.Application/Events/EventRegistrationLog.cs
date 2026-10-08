using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventRegistrationLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration list: {Count} registrations returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration found: {RegistrationId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration not found: {RegistrationId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid registrationId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration create: event {EventId} found in congregation {CongregationId}"
    )]
    public static partial void CreateEventFound(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration create failed: event {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateEventNotFound(
        ILogger logger,
        Guid congregationId,
        Guid eventId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration create: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration create failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration update: event {EventId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateEventFound(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration update failed: event {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateEventNotFound(
        ILogger logger,
        Guid congregationId,
        Guid eventId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration update: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration update failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration created: {RegistrationId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration updated: {RegistrationId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration update failed: {RegistrationId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(
        ILogger logger,
        Guid congregationId,
        Guid registrationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event registration deleted: {RegistrationId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid registrationId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration delete failed: {RegistrationId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(
        ILogger logger,
        Guid congregationId,
        Guid registrationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event registration create failed: member {MemberId} is already registered for event {EventId} in congregation {CongregationId}"
    )]
    public static partial void CreateDuplicate(
        ILogger logger,
        Guid congregationId,
        Guid eventId,
        Guid memberId
    );

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Event registration error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
