using Echo.Application.Services.Email;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Extensions;

public static class RegisterApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IIdGenerator, IdGenerator>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IPasswordHasher, BcryptHashService>();
        services.AddSingleton<ITokenHasher, Sha256HashService>();
        services.AddKeyedScoped<IEmailService, ResendEmailService>("Resend");

        return services;
    }
}
