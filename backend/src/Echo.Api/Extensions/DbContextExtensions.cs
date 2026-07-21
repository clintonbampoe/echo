using Echo.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Api.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = DbConnectionStringBuilder.Build(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.SetPostgresVersion(18, 0);
                    npgsqlOptions.MigrationsAssembly("Echo.Infrastructure");
                }
            )
        );

        return services;
    }
}
