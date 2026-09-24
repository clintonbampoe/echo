using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Api.Extensions;

public static class Database
{
    public static IServiceCollection ConfigureDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("echodb");
        Console.WriteLine($"ConnectionString: {connectionString}");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

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
