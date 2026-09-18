namespace Echo.Application.Options.Jwt;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Base64-encoded PEM of the key that signs new access tokens.</summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>Base64-encoded PEM of the public half of <see cref="PrivateKey"/>.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Base64-encoded PEM of the public key that was active before the last rotation.
    /// Set it for the duration of a rotation grace period so access tokens signed with the old
    /// key keep validating, then clear it once the grace period has passed.
    /// See the key rotation runbook in docs/Infrastructure.md.
    /// </summary>
    public string? PreviousPublicKey { get; set; }

    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}
