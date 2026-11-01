using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Shared.Caching.Extensions;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Registers the caching abstraction and in-memory implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (binds to "Cache" section).</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Registers:
    /// <list type="bullet">
    ///   <item><description><see cref="CacheOptions"/> bound to configuration section "Cache"</description></item>
    ///   <item><description><see cref="IMemoryCache"/> (required by <see cref="InMemoryCacheService"/>)</description></item>
    ///   <item><description><see cref="ICacheService"/> → <see cref="InMemoryCacheService"/> (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.Section));
        services.AddMemoryCache();
        services.AddScoped<ICacheService, InMemoryCacheService>();
        return services;
    }
}
