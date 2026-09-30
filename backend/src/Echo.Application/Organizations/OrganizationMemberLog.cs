using Microsoft.Extensions.Logging;

namespace Echo.Application.Organizations;

public static partial class OrganizationMemberLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Org member created: {OrgMemberId} successfully persisted"
    )]
    public static partial void Created(ILogger logger, Guid orgMemberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Org member updated: {OrgMemberId} successfully modified"
    )]
    public static partial void Updated(ILogger logger, Guid orgMemberId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Org member deleted: {OrgMemberId} soft-deleted"
    )]
    public static partial void Deleted(ILogger logger, Guid orgMemberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Org member not found: {OrgMemberId} requested"
    )]
    public static partial void NotFound(ILogger logger, Guid orgMemberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Org member creation failed: Member {MemberId} not found"
    )]
    public static partial void MemberNotFound(ILogger logger, Guid memberId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Org member creation failed: Organization {OrganizationId} not found"
    )]
    public static partial void OrganizationNotFound(ILogger logger, Guid organizationId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Org member error: unexpected failure - {Message}"
    )]
    public static partial void Error(ILogger logger, Exception ex, string message);
}
