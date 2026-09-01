using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Echo.Api.Tests;

internal static class JwtTestHelper
{
    internal const string Issuer = "test-issuer";
    internal const string Audience = "test-audience";

    internal static (string PrivateKeyB64, string PublicKeyB64) GenerateRsaKeyPair()
    {
        using var rsa = RSA.Create(2048);

        var privateB64 = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(rsa.ExportRSAPrivateKeyPem())
        );
        var publicB64 = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(rsa.ExportSubjectPublicKeyInfoPem())
        );

        return (privateB64, publicB64);
    }

    internal static IConfiguration BuildConfiguration(string privateKeyB64, string publicKeyB64)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:PrivateKey"] = privateKeyB64,
                    ["Jwt:PublicKey"] = publicKeyB64,
                }
            )
            .Build();
    }

    internal static string CreateSignedToken(string privateKeyB64)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyB64)));

        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        );

        var claims = new[]
        {
            new Claim("sub", "user-id"),
            new Claim("role", "Admin"),
            new Claim("congregationId", "congregation-id"),
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: signingCredentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    internal static async Task<TokenValidationResult> ValidateTokenAsync(
        string token,
        string publicKeyB64
    )
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(Convert.FromBase64String(publicKeyB64)));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };

        var handler = new JsonWebTokenHandler { MapInboundClaims = false };
        return await handler.ValidateTokenAsync(token, validationParameters);
    }
}
