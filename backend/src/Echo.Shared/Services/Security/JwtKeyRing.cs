using System.Security.Cryptography;
using System.Text;
using Echo.Shared.Options.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Echo.Shared.Services.Security;

/// <summary>
/// Every RSA key the API currently trusts: the active pair that signs new access tokens, plus
/// any retired public key still inside its rotation grace period.
/// <para>
/// Each key carries a <c>kid</c> derived from its own public material, so tokens identify which
/// key validates them without anyone having to maintain a kid naming convention across
/// environments. Both the signer and the JWT bearer handler read their keys from here, so the
/// two can never drift apart.
/// </para>
/// </summary>
public sealed class JwtKeyRing : IDisposable
{
    // 16 base64url characters of a SHA-256 thumbprint. Long enough that two keys will never
    // collide in practice, short enough to eyeball against a log line.
    private const int _keyIdLength = 16;

    private readonly List<RSA> _ownedKeys = [];

    // Signature providers are cached by key id + algorithm. Our key ids are derived from the key
    // material, so two rings built from the same config produce identical ids — and on the shared
    // CryptoProviderFactory.Default cache they would collide, handing one ring a provider that
    // wraps another ring's already-disposed RSA. A per-ring factory keeps the caching benefit
    // without the cross-talk.
    private readonly CryptoProviderFactory _cryptoProviderFactory = new();

    public JwtKeyRing(JwtOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var activeKey = CreateSecurityKey(Import(options.PrivateKey, "Jwt:PrivateKey"));
        EnsurePublicHalfMatches(activeKey, options.PublicKey);

        SigningCredentials = new SigningCredentials(activeKey, SecurityAlgorithms.RsaSha256);

        // The active key validates as well as signs — its public half is what the handler
        // checks signatures against.
        var validationKeys = new List<SecurityKey> { activeKey };

        if (!string.IsNullOrWhiteSpace(options.PreviousPublicKey))
        {
            var previousKey = CreateSecurityKey(
                Import(options.PreviousPublicKey, "Jwt:PreviousPublicKey")
            );

            if (previousKey.KeyId == activeKey.KeyId)
            {
                throw new InvalidOperationException(
                    "Jwt:PreviousPublicKey is the same key as Jwt:PublicKey. A rotation grace "
                        + "period needs the key that was active before the rotation, or no previous "
                        + "key at all."
                );
            }

            validationKeys.Add(previousKey);
        }

        ValidationKeys = validationKeys;
    }

    /// <summary>Credentials used to sign new access tokens.</summary>
    public SigningCredentials SigningCredentials { get; }

    /// <summary>
    /// Every key an incoming token may be signed with — the active key first, then any key
    /// still inside its grace period.
    /// </summary>
    public IReadOnlyList<SecurityKey> ValidationKeys { get; }

    /// <summary>The <c>kid</c> written into the header of every token this ring signs.</summary>
    public string ActiveKeyId => SigningCredentials.Key.KeyId;

    public void Dispose()
    {
        // Drop the cached providers before the keys they wrap, so nothing can reach a disposed RSA.
        if (_cryptoProviderFactory.CryptoProviderCache is IDisposable cache)
            cache.Dispose();

        foreach (var rsa in _ownedKeys)
            rsa.Dispose();

        _ownedKeys.Clear();
    }

    /// <summary>
    /// Derives a stable key id from the public half of <paramref name="rsa"/>. The same key
    /// always yields the same id and no two distinct keys share one, so a rotation can never
    /// accidentally reuse an id for different key material.
    /// </summary>
    private static string ComputeKeyId(RSA rsa)
    {
        var thumbprint = SHA256.HashData(rsa.ExportSubjectPublicKeyInfo());
        return Base64UrlEncoder.Encode(thumbprint)[.._keyIdLength];
    }

    /// <summary>
    /// Guards against the classic rotation slip of updating one half of the pair and leaving
    /// the other behind, which would otherwise only surface as tokens failing to validate.
    /// </summary>
    private static void EnsurePublicHalfMatches(
        RsaSecurityKey activeKey,
        string configuredPublicKey
    )
    {
        using var configured = Import(configuredPublicKey, "Jwt:PublicKey");

        if (ComputeKeyId(configured) != activeKey.KeyId)
        {
            throw new InvalidOperationException(
                "Jwt:PublicKey is not the public half of Jwt:PrivateKey. They must belong to the "
                    + "same RSA pair — check that a rotation did not leave one of them behind."
            );
        }
    }

    // JWT keys are stored base64-encoded rather than as raw PEM because PEM line breaks do not
    // survive the .env format. See docs/GettingStarted.md.
    private static RSA Import(string base64Pem, string settingName)
    {
        if (string.IsNullOrWhiteSpace(base64Pem))
        {
            throw new InvalidOperationException(
                $"{settingName} is not configured. Generate a JWT key pair with "
                    + "backend/tools/jwt-key-setup/SetupJwtKeys.sh — see docs/GettingStarted.md."
            );
        }

        string pem;
        try
        {
            pem = Encoding.UTF8.GetString(Convert.FromBase64String(base64Pem));
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"{settingName} is not valid base64. JWT keys are stored base64-encoded, not as "
                    + "raw PEM — see docs/GettingStarted.md.",
                ex
            );
        }

        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(pem);
        }
        catch (Exception ex) when (ex is ArgumentException or CryptographicException)
        {
            rsa.Dispose();
            throw new InvalidOperationException(
                $"{settingName} does not contain a valid PEM-encoded RSA key.",
                ex
            );
        }

        return rsa;
    }

    private RsaSecurityKey CreateSecurityKey(RSA rsa)
    {
        _ownedKeys.Add(rsa);
        return new RsaSecurityKey(rsa)
        {
            KeyId = ComputeKeyId(rsa),
            CryptoProviderFactory = _cryptoProviderFactory,
        };
    }
}
