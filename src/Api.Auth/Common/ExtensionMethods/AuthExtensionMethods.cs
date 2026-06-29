using Api.Auth.Services;
using Api.Auth.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Api.Auth.Common.ExtensionMethods;

public static class AuthExtensionMethods
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddResend(options =>
        {
            options.ApiToken = configuration["RESEND_API_TOKEN"]!;
        });

        services.AddScoped<IEmailService, ResendEmailService>();

        return services;
    }
}
