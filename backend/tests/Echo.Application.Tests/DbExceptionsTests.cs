using Echo.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Echo.Application.Tests;

[Trait("Category", "Unit")]
public class DbExceptionsTests
{
    private static DbUpdateException Failed(Exception? inner) => new("save failed", inner);

    private static PostgresException Postgres(string sqlState) =>
        new("error", "ERROR", "ERROR", sqlState);

    [Fact]
    public void IsUniqueViolation_IsTrue_ForAPostgresUniqueViolation()
    {
        Assert.True(Failed(Postgres(PostgresErrorCodes.UniqueViolation)).IsUniqueViolation());
    }

    [Fact]
    public void IsUniqueViolation_IsFalse_ForOtherDatabaseErrors()
    {
        Assert.False(Failed(Postgres(PostgresErrorCodes.ForeignKeyViolation)).IsUniqueViolation());
    }

    [Fact]
    public void IsUniqueViolation_IsFalse_WhenThereIsNoDatabaseErrorInside()
    {
        Assert.False(Failed(null).IsUniqueViolation());
        Assert.False(Failed(new InvalidOperationException()).IsUniqueViolation());
    }
}
