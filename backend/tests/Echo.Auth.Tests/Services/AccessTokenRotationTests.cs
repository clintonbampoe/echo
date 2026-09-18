using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Echo.Application.Options.Jwt;
using Echo.Application.Services.Security;
using Echo.Auth.Services;
using Echo.Core.Dtos;
using Echo.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Echo.Auth.Tests.Services;

/// <summary>
/// Covers the rotation story end to end: sign with one key, validate against a ring that may
/// hold a second, and confirm a key drops out of circulation once it leaves the ring.
/// </summary>
[Trait("Category", "Unit")]
public class AccessTokenRotationTests
{
    private static readonly KeyPair Current = GenerateKeyPair();
    private static readonly KeyPair Previous = GenerateKeyPair();
    private static readonly KeyPair Unrelated = GenerateKeyPair();

    private static readonly UserAuthDto User = new()
    {
        Id = Guid.CreateVersion7(),
        EmailAddress = "member@example.test",
        Name = "Test Member",
        Role = UserRole.Admin,
        CongregationId = Guid.CreateVersion7(),
    };

    [Fact]
    public void GeneratedToken_CarriesTheActiveKeyIdInItsHeader()
    {
        var options = OptionsFor(Current);
        using var keyRing = new JwtKeyRing(options);

        var token = CreateGenerator(options, keyRing).Generate(User).Token;

        Assert.Equal(keyRing.ActiveKeyId, new JsonWebToken(token).Kid);
    }

    [Fact]
    public async Task TokenSignedWithTheActiveKey_Validates()
    {
        var options = OptionsFor(Current);

        var result = await ValidateAsync(IssueToken(options), options);

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task TokenSignedWithTheOldKey_StillValidates_DuringTheGracePeriod()
    {
        // Before rotation: tokens are signed with the key that is about to be retired.
        var token = IssueToken(OptionsFor(Previous));

        // After rotation: a new active key, with the retired one kept on the ring for the grace
        // period. Tokens already in users' hands must keep working — that is the whole point.
        var afterRotation = OptionsFor(Current, Previous);

        var result = await ValidateAsync(token, afterRotation);

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task TokenSignedWithTheOldKey_IsRejected_OnceTheGracePeriodEnds()
    {
        var token = IssueToken(OptionsFor(Previous));

        // Grace period over: the retired public key is removed from config.
        var result = await ValidateAsync(token, OptionsFor(Current));

        Assert.False(result.IsValid);
        Assert.IsType<SecurityTokenSignatureKeyNotFoundException>(result.Exception);
    }

    [Fact]
    public async Task TokenSignedWithAKeyTheRingNeverHeld_IsRejected()
    {
        var token = IssueToken(OptionsFor(Unrelated));

        var result = await ValidateAsync(token, OptionsFor(Current, Previous));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task TokenCarryingNoKid_StillValidates()
    {
        // Access tokens minted before kid support carry no kid at all. They have to keep working
        // across the deploy of this change, or shipping it logs out every signed-in user.
        var options = OptionsFor(Current);

        var result = await ValidateAsync(IssueTokenWithoutKid(options), options);

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    private static AccessTokenGenerator CreateGenerator(JwtOptions options, JwtKeyRing keyRing) =>
        new(Options.Create(options), TimeProvider.System, keyRing);

    private static string IssueToken(JwtOptions options)
    {
        using var keyRing = new JwtKeyRing(options);
        return CreateGenerator(options, keyRing).Generate(User).Token;
    }

    /// <summary>Signs the way the API did before this key ring existed — no kid in the header.</summary>
    private static string IssueTokenWithoutKid(JwtOptions options)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(Convert.FromBase64String(options.PrivateKey)));

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: [new Claim("sub", User.Id.ToString())],
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenLifetimeMinutes),
            signingCredentials: new SigningCredentials(
                // Own provider cache, for the same reason JwtKeyRing keeps one.
                new RsaSecurityKey(rsa) { CryptoProviderFactory = new CryptoProviderFactory() },
                SecurityAlgorithms.RsaSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<TokenValidationResult> ValidateAsync(string token, JwtOptions options)
    {
        using var keyRing = new JwtKeyRing(options);

        return await new JsonWebTokenHandler().ValidateTokenAsync(
            token,
            JwtTokenValidation.CreateParameters(options, keyRing)
        );
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
