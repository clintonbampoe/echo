using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Seeder.Core;

/// <summary>
/// The two checks the seeder makes before it writes anything: clear the
/// database (Reset=true) or refuse to run on a database that has data.
/// </summary>
public static class SeedDatabase
{
    /// <summary>Throws if the database already has a congregation.</summary>
    public static async Task EnsureEmptyAsync(AppDbContext db, CancellationToken ct)
    {
        var count = await db.Congregations.CountAsync(ct);
        if (count > 0)
            throw new InvalidOperationException(
                $"Database already has {count} congregation(s). Pass Reset=true to clear before seeding."
            );
    }

    /// <summary>
    /// Empties every table the seeder writes to. TRUNCATE CASCADE clears each
    /// named table and every table that references it, so the order does not
    /// matter. Leaf tables are listed first anyway, in case CASCADE is removed.
    /// </summary>
    public static async Task ResetAsync(AppDbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                assets,
                asset_categories,
                project_contributions,
                projects,
                project_categories,
                tithes,
                transactions,
                transaction_categories,
                event_attendances,
                event_registrations,
                events,
                attendance,
                sessions,
                session_labels,
                organization_members,
                organizations,
                visitors,
                members,
                people,
                users,
                congregations
            RESTART IDENTITY CASCADE
            """,
            ct
        );
    }
}
