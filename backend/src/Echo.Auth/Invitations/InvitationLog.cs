using Echo.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Invitations;

public static partial class InvitationLog
{
    // --- Create ---
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Invitation create succeeded: token created for congregation {CongregationId} by user {CreatedByUserId} with role {AllowedRole}"
    )]
    public static partial void InvitationCreateSucceeded(
        ILogger logger,
        Guid congregationId,
        Guid createdByUserId,
        UserRole allowedRole // enum, not string
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Invitation create error: unexpected failure for congregation {CongregationId} by user {CreatedByUserId}"
    )]
    public static partial void InvitationCreateFailed(
        ILogger logger,
        Exception ex,
        Guid congregationId,
        Guid createdByUserId
    );

    // --- Validate ---
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Invitation validation failed: token not found"
    )]
    public static partial void InvitationValidationFailedNotFound(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Invitation validation failed: token revoked for congregation {CongregationId}"
    )]
    public static partial void InvitationValidationFailedRevoked(
        ILogger logger,
        Guid congregationId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Invitation validation failed: token expired for congregation {CongregationId}"
    )]
    public static partial void InvitationValidationFailedExpired(
        ILogger logger,
        Guid congregationId
    );
}
