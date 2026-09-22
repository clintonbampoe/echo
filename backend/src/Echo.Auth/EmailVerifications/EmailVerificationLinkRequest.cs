namespace Echo.Auth.EmailVerifications;

public record EmailVerificationLinkRequest
{
    public string Email { get; init; } = string.Empty;
}
