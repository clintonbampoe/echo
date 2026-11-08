using Echo.Application.Caching;
using Echo.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Echo.Api.Extensions;

public static class Database
{
    // The API's pool of Postgres connections. Ten stay open, so a burst of requests after a quiet spell does not
    // wait to open them (a new connection took 50 to 400 ms under load). The ceiling stays well below what the
    // database accepts (max_connections is 40, three of them reserved for the superuser): at the old 100 the
    // overloaded API asked for connections the database refused ("too many clients") and answered 500. A full pool
    // makes the surplus requests wait for a free connection, which is slow but not an error. The 500-user test
    // never used more than 14.
    private const int _minPoolSize = 10;
    private const int _maxPoolSize = 30;

    // A statement used twice is prepared on its connection, so Postgres stops parsing and planning it for every
    // request (about half a millisecond each, on queries that run in two). 64 covers the endpoints' distinct
    // statements; a connection keeps its own set.
    private const int _autoPrepareMinUsage = 2;
    private const int _maxAutoPrepare = 64;

    internal static string WithPoolSize(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString)
        {
            MinPoolSize = _minPoolSize,
            MaxPoolSize = _maxPoolSize,
            AutoPrepareMinUsages = _autoPrepareMinUsage,
            MaxAutoPrepare = _maxAutoPrepare,
        }.ConnectionString;

    public static IServiceCollection ConfigureDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var connectionString = WithPoolSize(configuration.GetConnectionString("echodb")!);

        // Pooled: the context object is reused between requests instead of built for each one.
        services.AddSingleton<CacheEvictionInterceptor>();
        services.AddDbContextPool<AppDbContext>(
            (provider, options) =>
            {
                options.UseNpgsql(connectionString).ConfigureEchoDatabase();
                options.AddInterceptors(provider.GetRequiredService<CacheEvictionInterceptor>());

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            }
        );

        services.AddHostedService<StartupWarmup>();

        return services;
    }

    public static async Task RunMigrationsOnStartup(
        WebApplication app,
        IConfiguration configuration
    )
    {
        if (configuration.GetValue<bool>("RunMigrationsOnStartup"))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
