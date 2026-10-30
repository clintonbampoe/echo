using Echo.Shared.Models;

namespace Echo.Shared.Services.Email;

/// <summary>
/// One email to send. <paramref name="Kind"/> (for example <c>email_verification</c>) and the ids
/// are only used to describe the message in logs when delivery fails.
/// </summary>
public sealed record EmailMessage(
    string To,
    IEmailContent Content,
    string Kind,
    Guid? UserId = null,
    Guid? CongregationId = null
);
