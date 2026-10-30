using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.EmailVerifications;

public record VerificationStatusDto
{
    public required bool Verified { get; init; }

    /// <summary>When the email address was verified; null while it is not.</summary>
    public DateTime? VerifiedAt { get; init; }

    /// <summary>The address a verification link is sent to.</summary>
    public required string EmailAddress { get; init; }
}

public record EmailVerificationLinkRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}
