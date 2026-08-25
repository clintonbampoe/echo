using Echo.Application.Services;
using Echo.Application.Services.Email;
using Echo.Application.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Extensions;

public static class RegisterApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IPasswordHasher, BcryptHashService>();
        services.AddScoped<ITokenHasher, Sha256HashService>();
        services.AddKeyedScoped<IEmailService, ResendEmailService>("Resend");

        return services;
    }
}
