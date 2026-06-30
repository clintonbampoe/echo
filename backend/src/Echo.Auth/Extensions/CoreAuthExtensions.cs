using Echo.Auth.Services;
using Echo.Auth.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Echo.Auth.Extensions;

public static class CoreAuthExtensions
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

        // first parameter is ignored because we have no configurations outside our AutoMapper profiles
        services.AddAutoMapper(_ => { } , typeof(CoreAuthExtensions));
        return services;
    }
}
