using System.Security.Cryptography;
using System.Text;
using Echo.Application.Options.Jwt;
using Echo.Application.Services.Security;

namespace Echo.Application.Tests.Services.Security;

[Trait("Category", "Unit")]
public class JwtKeyRingTests
{
    // Generating RSA keys is slow, so the suite shares a few fixed pairs rather than minting
    // one per test.
    private static readonly KeyPair Current = GenerateKeyPair();
    private static readonly KeyPair Previous = GenerateKeyPair();
    private static readonly KeyPair Unrelated = GenerateKeyPair();

    [Fact]
    public void ActiveKeyId_IsDerivedFromTheKeyItself()
    {
        using var first = new JwtKeyRing(OptionsFor(Current));
        using var second = new JwtKeyRing(OptionsFor(Current));

        // Nothing in config names the key, so the same key material must always produce the same
        // id — otherwise a restart would orphan every token it had already signed.
        Assert.Equal(first.ActiveKeyId, second.ActiveKeyId);
        Assert.NotEmpty(first.ActiveKeyId);
    }

    [Fact]
    public void ActiveKeyId_DiffersBetweenDifferentKeys()
    {
        using var current = new JwtKeyRing(OptionsFor(Current));
        using var previous = new JwtKeyRing(OptionsFor(Previous));

        Assert.NotEqual(current.ActiveKeyId, previous.ActiveKeyId);
    }

    [Fact]
    public void ValidationKeys_HoldsOnlyTheActiveKey_WhenNoRotationIsInFlight()
    {
        using var keyRing = new JwtKeyRing(OptionsFor(Current));

        var key = Assert.Single(keyRing.ValidationKeys);
        Assert.Equal(keyRing.ActiveKeyId, key.KeyId);
    }

    [Fact]
    public void ValidationKeys_HoldsBothKeys_DuringARotationGracePeriod()
    {
        using var keyRing = new JwtKeyRing(OptionsFor(Current, Previous));

        Assert.Equal(2, keyRing.ValidationKeys.Count);
        Assert.Contains(keyRing.ValidationKeys, key => key.KeyId == keyRing.ActiveKeyId);
    }

    [Fact]
    public void Constructor_Rejects_APublicKeyThatIsNotTheHalfOfThePrivateKey()
    {
        // The half-finished rotation: new private key deployed, old public key left behind.
        var options = OptionsFor(Current);
        options.PublicKey = Unrelated.PublicKey;

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyRing(options));

        Assert.Contains("same RSA pair", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_Rejects_APreviousKeyThatIsStillTheActiveKey()
    {
        var options = OptionsFor(Current);
        options.PreviousPublicKey = Current.PublicKey;

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyRing(options));

        Assert.Contains("same key", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Rejects_AMissingPrivateKey(string privateKey)
    {
        var options = OptionsFor(Current);
        options.PrivateKey = privateKey;

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyRing(options));

        Assert.Contains(
            "Jwt:PrivateKey is not configured",
            exception.Message,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Constructor_Rejects_ARawPemKeyThatWasNotBase64Encoded()
    {
        // The mistake documented in docs/Infrastructure.md — raw PEM pasted straight into .env.
        var options = OptionsFor(Current);
        options.PrivateKey =
            "-----BEGIN RSA PRIVATE KEY-----\nnot base64\n-----END RSA PRIVATE KEY-----";

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyRing(options));

        Assert.Contains("not valid base64", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_Rejects_ValidBase64ThatIsNotAKey()
    {
        var options = OptionsFor(Current);
        options.PrivateKey = Convert.ToBase64String("definitely not a PEM key"u8.ToArray());

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyRing(options));

        Assert.Contains("valid PEM-encoded RSA key", exception.Message, StringComparison.Ordinal);
    }

    private static JwtOptions OptionsFor(KeyPair active, KeyPair? previous = null) =>
        new()
        {
            Issuer = "echo-api-test",
            Audience = "echo-clients-test",
            PrivateKey = active.PrivateKey,
            PublicKey = active.PublicKey,
            PreviousPublicKey = previous?.PublicKey,
        };

    /// <summary>
    /// Produces a pair in the same shape the API consumes: base64-wrapped PEM, exactly as
    /// backend/tools/jwt-key-setup/GenerateJwtKeys.cs writes the real ones.
    /// </summary>
    private static KeyPair GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);

        return new KeyPair(
            Convert.ToBase64String(Encoding.UTF8.GetBytes(rsa.ExportRSAPrivateKeyPem())),
            Convert.ToBase64String(Encoding.UTF8.GetBytes(rsa.ExportSubjectPublicKeyInfoPem()))
        );
    }

    private sealed record KeyPair(string PrivateKey, string PublicKey);
}
