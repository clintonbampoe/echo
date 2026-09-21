using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Echo.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = LoadConfiguration();
            var connectionString = ConnectionStringBuilder.Build(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsAssembly("Echo.Data"));

            return new AppDbContext(optionsBuilder.Options);
        }

        private IConfiguration LoadConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
