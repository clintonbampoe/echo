using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public static partial class EventLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event list: {Count} events returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Search ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event search: {Count} results for query '{Query}' in congregation {CongregationId}"
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
        Message = "Event found: {EventId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event not found: {EventId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid eventId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event create: organizer {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizerFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event create failed: organizer {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizerNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event create: organization {OrganizationId} found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizationFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event create failed: organization {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizationNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event update: organizer {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizerFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event update failed: organizer {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizerNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event update: organization {OrganizationId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizationFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event update failed: organization {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizationNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event created: {EventId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event updated: {EventId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event update failed: {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Event deleted: {EventId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Event delete failed: {EventId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid eventId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Event error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
