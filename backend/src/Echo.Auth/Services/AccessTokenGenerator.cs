using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Echo.Application.Options.Jwt;
using Echo.Application.Services.Security;
using Echo.Core.Dtos;
using Microsoft.Extensions.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Echo.Auth.Services;

public class AccessTokenGenerator(
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider,
    JwtKeyRing keyRing
)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public (string Token, DateTime ExpiresAt) Generate(UserAuthDto user)
    {
        var expiresAt = timeProvider
            .GetUtcNow()
            .UtcDateTime.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("congregationId", user.CongregationId.ToString()),
        };

        // JwtSecurityToken copies the signing key's KeyId into the header as `kid`. That is what
        // lets validation pick the right key out of the ring during a rotation grace period.
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: keyRing.SigningCredentials
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expiresAt);
    }
}
