using System.Security.Cryptography;
using System.Text;
using Echo.Application.Options;
using Echo.Auth.Services;
using Echo.Core.Dtos;
using Echo.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Auth.Tests;

public class AccessTokenGeneratorTests
{
    [Fact]
    public void Generate_UsesTimeProviderForExpiry()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
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

    private static AccessTokenGenerator CreateGenerator(TimeProvider timeProvider, int accessTokenLifetimeMinutes)
    {
        using var rsa = RSA.Create(2048);
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        var privateKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyPem));

        var options = Options.Create(
            new JwtOptions
            {
                PrivateKey = privateKeyBase64,
                PublicKey = "unused-in-generate",
                Issuer = "echo-api-test",
                Audience = "echo-clients-test",
                AccessTokenLifetimeMinutes = accessTokenLifetimeMinutes,
                RefreshTokenLifetimeDays = 30,
            }
        );

        return new AccessTokenGenerator(options, timeProvider);
    }
}
