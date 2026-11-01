using Echo.Api.Extensions;
using Npgsql;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class DatabasePoolTests
{
    [Fact]
    public void ThePool_KeepsTenConnectionsOpen_AndNeverMoreThanThirty()
    {
        var result = new NpgsqlConnectionStringBuilder(
            Database.WithPoolSize("Host=db;Port=5432;Database=echo_db;Username=u;Password=p")
        );

        Assert.Equal(10, result.MinPoolSize);
        Assert.Equal(30, result.MaxPoolSize);
    }

    [Fact]
    public void AStatementUsedTwice_IsPreparedOnItsConnection()
    {
        var result = new NpgsqlConnectionStringBuilder(
            Database.WithPoolSize("Host=db;Port=5432;Database=echo_db;Username=u;Password=p")
        );

        Assert.Equal(2, result.AutoPrepareMinUsages);
        Assert.Equal(64, result.MaxAutoPrepare);
    }

    [Fact]
    public void EverythingElseInTheConnectionString_IsKept()
    {
        var result = new NpgsqlConnectionStringBuilder(
            Database.WithPoolSize("Host=db;Port=5432;Database=echo_db;Username=u;Password=p")
        );

        Assert.Equal("db", result.Host);
        Assert.Equal("echo_db", result.Database);
        Assert.Equal("p", result.Password);
    }
}
