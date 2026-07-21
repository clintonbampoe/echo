using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Echo.Domain.Data;

public static class DbConnectionStringBuilder
{
    public static string Build(IConfiguration configuration)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["Database:Host"],
            Port = configuration.GetValue<int>("Database:Port"),
            Database = configuration["Database:Name"],
            Username = configuration["Database:Username"],
            Password = configuration["Database:Password"],
        };

        return builder.ConnectionString;
    }
}
