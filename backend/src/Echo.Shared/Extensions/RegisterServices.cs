using Echo.Shared.Caching.Extensions;
using Echo.Shared.Options.Frontend;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Echo.Shared.Extensions;

/// <summary>
/// Registers shared infrastructure services.
/// </summary>
public static class RegisterServices
{
    /// <summary>
    /// Adds shared services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (required for CacheOptions binding).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
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

        services.AddCaching(configuration);

        return services;
    }
}
