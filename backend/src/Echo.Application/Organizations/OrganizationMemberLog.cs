using Microsoft.Extensions.Logging;

namespace Echo.Application.Organizations;

public static partial class OrganizationMemberLog
{
    // --- List ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member list: {Count} members returned for congregation {CongregationId}"
    )]
    public static partial void Listed(ILogger logger, Guid congregationId, int count);

    // --- Get by id ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member found: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Found(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member not found: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void NotFound(ILogger logger, Guid congregationId, Guid memberId);

    // --- Create dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member create: organization {OrganizationId} found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizationFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member create failed: organization {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateOrganizationNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member create: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member create failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void CreateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Update dependencies ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member update: organization {OrganizationId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizationFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member update failed: organization {OrganizationId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateOrganizationNotFound(
        ILogger logger,
        Guid congregationId,
        Guid organizationId
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member update: member {MemberId} found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member update failed: member {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateMemberNotFound(
        ILogger logger,
        Guid congregationId,
        Guid memberId
    );

    // --- Outcomes ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member created: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Created(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member updated: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Updated(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member update failed: {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void UpdateNotFound(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Organization member deleted: {MemberId} in congregation {CongregationId}"
    )]
    public static partial void Deleted(ILogger logger, Guid congregationId, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Organization member delete failed: {MemberId} not found in congregation {CongregationId}"
    )]
    public static partial void DeleteNotFound(ILogger logger, Guid congregationId, Guid memberId);

    // --- Unexpected ---
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Organization member error: unexpected failure for congregation {CongregationId}"
    )]
    public static partial void Error(ILogger logger, Exception ex, Guid congregationId);
}
