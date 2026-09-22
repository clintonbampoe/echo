using Echo.Shared.Options.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Echo.Shared.Services.Security;

public static class JwtTokenValidation
{
    /// <summary>
    /// Builds the rules every incoming access token is checked against. Lives here rather than
    /// inline in the API's bearer setup so tests validate tokens exactly the way the running API
    /// does, instead of against a copy that can quietly drift.
    /// </summary>
    public static TokenValidationParameters CreateParameters(JwtOptions options, JwtKeyRing keyRing)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(keyRing);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // Plural: the handler resolves by the token's `kid`, so the active key and a key
            // inside its rotation grace period both validate. Tokens carrying no kid (issued
            // before kid support existed) still work because the handler falls back to trying
            // every key on the ring.
            IssuerSigningKeys = keyRing.ValidationKeys,

            NameClaimType = "sub",
            RoleClaimType = "role",
        };
    }
}
