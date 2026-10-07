using Echo.Auth.EmailVerifications;
using Echo.Auth.Invitations;
using Echo.Auth.Passwords;
using Echo.Auth.Registrations;
using Echo.Auth.Sessions;
using Echo.Shared.Options.Frontend;
using Echo.Shared.Options.Jwt;
using Echo.Shared.Services.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resend;

namespace Echo.Auth;

public static class Extensions
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddResend(options =>
        {
            options.ApiToken = configuration["Resend:ApiKey"]!;
        });
        services.AddScoped<IEmailService, ResendEmailService>();

        services.AddScoped<RegistrationService>();
        services.AddScoped<EmailVerificationRepository>();
        services.AddScoped<EmailVerificationService>();
        services.AddScoped<PasswordResetRepository>();
        services.AddScoped<PasswordResetService>();

        services.Configure<FrontendOptions>(configuration.GetSection("FrontendClient"));
        services.AddScoped<LinkBuilder>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // AccessTokenGenerator is a singleton because it is stateless
        services.AddSingleton<JwtTokenGenerator>();

        services.AddScoped<SessionService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<SessionRepository>();
        services.AddScoped<InvitationRepository>();
        services.AddScoped<InvitationService>();

        services.AddSingleton<AuthInstrumentation>();
        return services;
    }

    public static IHostApplicationBuilder ConfigureAuthObservability(
        this IHostApplicationBuilder builder
    )
    {
        builder
            .Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                AuthInstrumentation.ConfigureTracing(tracing);
            })
            .WithMetrics(metrics => AuthInstrumentation.ConfigureMetrics(metrics));
        return builder;
    }

    public static RouteGroupBuilder MapAuthEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var auth = group.MapGroup("/auth");
        auth.MapSessionEndpoints(instrumentation);
        auth.MapPasswordEndpoints(instrumentation);
        auth.MapInvitationEndpoints(instrumentation);
        auth.MapVerificationEndpoints(instrumentation);
        auth.MapRegistrationEndpoints(instrumentation);
        return group;
    }
}
