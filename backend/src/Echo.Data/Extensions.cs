using Echo.Data.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Data;

public static class Extensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IHealthCheck, DbHealthCheck>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
