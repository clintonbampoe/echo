using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.EmailVerifications;

public record EmailVerificationLinkRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}
