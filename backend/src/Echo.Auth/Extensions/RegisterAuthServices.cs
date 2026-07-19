using Echo.Application.Configuration;
using Echo.Application.Options;
using Echo.Application.Services.Email;
using Echo.Auth.Controllers;
using Echo.Auth.Repositories;
using Echo.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Echo.Auth.Extensions;

public static class RegisterAuthServices
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

        // first parameter is ignored because we have no configurations outside our AutoMapper profiles
        services.AddAutoMapper(_ => { }, typeof(RegisterAuthServices));

        services.AddScoped<RegistrationService>();
        services.AddScoped<RegisterController>();
        services.AddScoped<EmailVerificationTokenRepository>();
        services.AddScoped<EmailVerificationService>();

        services.Configure<FrontendClientOptions>(configuration.GetSection("FrontendClient"));
        services.AddScoped<AuthLinkBuilder>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // AccessTokenGenerator is a singleton because it is stateless
        services.AddSingleton<AccessTokenGenerator>();

        services.AddScoped<AuthenticationService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<RefreshTokenRepository>();

        return services;
    }
}
