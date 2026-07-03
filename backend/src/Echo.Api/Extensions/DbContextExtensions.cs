using Echo.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Echo.Api.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["Database:Host"],
            Port = configuration.GetValue<int>("Database:Port"),
            Database = configuration["Database:Name"],
            Username = configuration["Database:Username"],
            Password = configuration["Database:Password"],
        };

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionStringBuilder.ConnectionString,
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
