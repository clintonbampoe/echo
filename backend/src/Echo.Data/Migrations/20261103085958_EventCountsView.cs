using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventCountsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core cannot create a materialized view, so it is plain SQL. One row per event: how many registered and how many
            // came, which the event list reads instead of counting every registration and check-in. The unique index lets the
            // refresh run CONCURRENTLY, so readers are never blocked.
            migrationBuilder.Sql(
                """
                CREATE MATERIALIZED VIEW event_counts AS
                SELECT e.id AS event_id,
                       COALESCE(r.total, 0)::int AS registrations,
                       COALESCE(a.total, 0)::int AS attendees
                FROM events e
                LEFT JOIN (
                    SELECT event_id, count(*) AS total
                    FROM event_registrations
                    WHERE deleted_at IS NULL
                    GROUP BY event_id
                ) r ON r.event_id = e.id
                LEFT JOIN (
                    SELECT event_id, count(*) AS total
                    FROM event_attendances
                    WHERE deleted_at IS NULL
                    GROUP BY event_id
                ) a ON a.event_id = e.id
                WHERE e.deleted_at IS NULL
                """
            );
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_event_counts_event_id ON event_counts (event_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW event_counts");
        }
    }
}
