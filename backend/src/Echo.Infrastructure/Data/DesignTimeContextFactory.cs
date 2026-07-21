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
            var connectionString = DbConnectionStringBuilder.Build(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsAssembly("Echo.Infrastructure"));

            return new AppDbContext(optionsBuilder.Options);
        }

        private IConfiguration LoadConfiguration()
        {
            // Call your central .env loading logic here
            // e.g., YourExistingLoader.LoadEnvFiles();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets("<paste Echo.Api's UserSecretsId GUID here>")
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
