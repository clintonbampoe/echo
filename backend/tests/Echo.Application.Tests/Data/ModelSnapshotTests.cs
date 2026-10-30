using Echo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Tests.Data;

// The migrations are the history of the database; the snapshot is where they leave the model. If the model has moved
// on without a migration (or a migration was edited after it ran somewhere), `migrations add` would produce a
// surprise, and databases already migrated would not match the code. This fails the build instead, and says what
// differs.
[Trait("Category", "Unit")]
public class ModelSnapshotTests
{
    private static string Describe(MigrationOperation op)
    {
        string? Read(string name) => op.GetType().GetProperty(name)?.GetValue(op)?.ToString();
        return $"{op.GetType().Name} {Read("Table")} {Read("Name")}".Replace("  ", " ").Trim();
    }

    [Fact]
    public void TheModel_HasNoChangesThatTheMigrationsDoNotCover()
    {
        // Built the way the app builds it, or the naming convention would differ from the snapshot's.
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x");
        builder.ConfigureEchoDatabase();
        using var context = new AppDbContext(builder.Options);

        var snapshot = context.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        var before = context.GetService<IModelRuntimeInitializer>().Initialize(snapshot);
        var after = context.GetService<IDesignTimeModel>().Model;
        var changes = context
            .GetService<IMigrationsModelDiffer>()
            .GetDifferences(before.GetRelationalModel(), after.GetRelationalModel());

        Assert.True(
            changes.Count == 0,
            "The model differs from the migrations snapshot. Add a migration for: "
                + string.Join("; ", changes.Select(Describe))
        );
    }
}
