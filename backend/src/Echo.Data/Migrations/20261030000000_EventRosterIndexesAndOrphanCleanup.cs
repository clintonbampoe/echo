using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <summary>
    /// One event's registrations and check-ins in the order their lists show them, so a page reads only its own rows.
    /// Also drops an index some databases hold that no model or migration mentions: a leftover of
    /// DenormalizeSessionDate before it was edited. It is 340 MB on a large attendance table, never read, and slows
    /// every write. IF EXISTS makes it a no-op everywhere else.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261030000000_EventRosterIndexesAndOrphanCleanup")]
    public partial class EventRosterIndexesAndOrphanCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_event_registrations_event_id_registration_date_id",
                table: "event_registrations",
                columns: new[] { "event_id", "registration_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_event_attendances_event_id_check_in_time_id",
                table: "event_attendances",
                columns: new[] { "event_id", "check_in_time", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_attendance_congregation_id_session_date;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_event_registrations_event_id_registration_date_id",
                table: "event_registrations"
            );

            migrationBuilder.DropIndex(
                name: "ix_event_attendances_event_id_check_in_time_id",
                table: "event_attendances"
            );
        }
    }
}
