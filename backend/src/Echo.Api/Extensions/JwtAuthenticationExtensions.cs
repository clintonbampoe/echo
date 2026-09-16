using Echo.Application.Options.Jwt;
using Echo.Application.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Echo.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtOptions =
            configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

        // Built here rather than resolved from DI so misconfigured keys fail startup with a clear
        // message instead of surfacing on the first request that needs a token.
        var keyRing = new JwtKeyRing(jwtOptions);
        services.AddSingleton(keyRing);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT claim names as-is ("sub", "role", "congregationId") instead of
                // remapping to ClaimTypes.*. Scoped to this handler only — do NOT reintroduce
                // JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear(), which mutates global
                // state for every JsonWebTokenHandler in the process.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = JwtTokenValidation.CreateParameters(
                    jwtOptions,
                    keyRing
                );
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
