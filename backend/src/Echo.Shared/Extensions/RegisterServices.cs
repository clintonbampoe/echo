using Echo.Shared.Options.Frontend;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Echo.Shared.Extensions;

public static class RegisterServices
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IIdGenerator, IdGenerator>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IPasswordHasher, BcryptHashService>();
        services.AddSingleton<ITokenHasher, Sha256HashService>();
        services.AddSingleton<IEncoder, CursorEncoder>();
        services.AddKeyedScoped<IEmailService, ResendEmailService>("Resend");

        services
            .AddOptions<FrontendOptions>()
            .BindConfiguration(FrontendOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<FrontendOptions>, FrontendOptionsValidator>();

        return services;
    }
}
