using System.Security.Cryptography;
using System.Text;
using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Domain.Users;
using Echo.Shared.Options.Jwt;
using Echo.Shared.Services.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Auth.Tests.Services;

[Trait("Category", "Unit")]
public class AccessTokenGeneratorTests
{
    [Fact]
    public void Generate_UsesTimeProviderForExpiry()
    {
        var fakeTime = new FakeTimeProvider(
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        );
        var generator = CreateGenerator(fakeTime, accessTokenLifetimeMinutes: 15);

        var user = new UserAuthDto
        {
            Id = Guid.CreateVersion7(),
            EmailAddress = "test@example.com",
            Name = "Test User",
            Role = UserRole.Admin,
            CongregationId = Guid.CreateVersion7(),
        };

        var (_, expiresAt) = generator.Generate(user);

        Assert.Equal(new DateTime(2026, 1, 1, 12, 15, 0, DateTimeKind.Utc), expiresAt);

        fakeTime.Advance(TimeSpan.FromMinutes(20));
        var (_, laterExpiresAt) = generator.Generate(user);

        Assert.Equal(new DateTime(2026, 1, 1, 12, 35, 0, DateTimeKind.Utc), laterExpiresAt);
    }

    private static JwtTokenGenerator CreateGenerator(
        TimeProvider timeProvider,
        int accessTokenLifetimeMinutes
    )
    {
        using var rsa = RSA.Create(2048);
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        var privateKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyPem));

        // The public half is no longer "unused": JwtKeyRing checks it really is the other half
        // of the private key, so a half-finished rotation fails at startup rather than silently
        // rejecting every token.
        var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
        var publicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyPem));

        var jwtOptions = new JwtOptions
        {
            PrivateKey = privateKeyBase64,
            PublicKey = publicKeyBase64,
            Issuer = "echo-api-test",
            Audience = "echo-clients-test",
            AccessTokenLifetimeMinutes = accessTokenLifetimeMinutes,
            RefreshTokenLifetimeDays = 30,
        };

        return new JwtTokenGenerator(
            Options.Create(jwtOptions),
            timeProvider,
            new JwtKeyRing(jwtOptions)
        );
    }
}
