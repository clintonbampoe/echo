using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Echo.Application.Options;
using Echo.Core.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Echo.Auth.Services;

public class AccessTokenGenerator
{
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _signingCredentials;

    public AccessTokenGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        var rsa = RSA.Create();

        rsa.ImportFromPem(_jwtOptions.PrivateKey);
        _signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }

    public (string Token, DateTime ExpiresAt) Generate(UserAuthDto user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("congregationId", user.CongregationId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expiresAt);
    }
}
