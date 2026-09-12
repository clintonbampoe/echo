using Echo.Application.Services.Email;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Extensions.DI;

public static class RegisterApplicationServices
{
    public static IServiceCollection InjectApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IIdGenerator, IdGenerator>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IPasswordHasher, BcryptHashService>();
        services.AddSingleton<ITokenHasher, Sha256HashService>();
        services.AddSingleton<IEncoder, CursorEncoder>();
        services.AddKeyedScoped<IEmailService, ResendEmailService>("Resend");

        return services;
    }
}
