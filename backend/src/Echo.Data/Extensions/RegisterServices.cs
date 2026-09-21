using Echo.Data.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Data.Extensions;

public static class RegisterServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IHealthCheck, DbHealthCheck>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
