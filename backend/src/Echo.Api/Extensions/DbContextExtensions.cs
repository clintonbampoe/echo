using Echo.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Api.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContextServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var template =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection template not found.");

        var connectionString = template
            .Replace("__DB_USER__", configuration["DB_USER"])
            .Replace("__DB_PASSWORD__", configuration["DB_PASSWORD"]);

        // REGISTER SERVICES
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Echo.Infrastructure"))
        );

        return services;
    }
}
