using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Echo.Domain.Data;

namespace Echo.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = LoadConfiguration();
            var connectionString = GetConfiguredConnectionString(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsAssembly("Echo.Infrastructure"));

            return new AppDbContext(optionsBuilder.Options);
        }

        private IConfiguration LoadConfiguration()
        {
            // Call your central .env loading logic here
            // e.g., YourExistingLoader.LoadEnvFiles();

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables() // Natively pulls flat environment variables
                .Build();
        }

        private string GetConfiguredConnectionString(IConfiguration configuration)
        {
            var template = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(template))
            {
                throw new InvalidOperationException("The 'DefaultConnection' template was not found in appsettings.");
            }

            // Pull flat secrets from your env loader and substitute them into the template
            return template
                .Replace("__DB_USER__", configuration["DB_USER"])
                .Replace("__DB_PASSWORD__", configuration["DB_PASSWORD"]);
        }
    }
}
