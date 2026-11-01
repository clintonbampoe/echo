using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventsSummariesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_congregation_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_start_date_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_event_registrations_congregation_id",
                table: "event_registrations");

            migrationBuilder.DropIndex(
                name: "ix_event_registrations_registration_date_id",
                table: "event_registrations");

            migrationBuilder.DropIndex(
                name: "ix_event_attendances_check_in_time_id",
                table: "event_attendances");

            migrationBuilder.DropIndex(
                name: "ix_event_attendances_congregation_id",
                table: "event_attendances");

            migrationBuilder.CreateIndex(
                name: "ix_events_congregation_id_start_date_id",
                table: "events",
                columns: new[] { "congregation_id", "start_date", "id" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "organization_id", "organizer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_event_registrations_congregation_id_registration_date_id",
                table: "event_registrations",
                columns: new[] { "congregation_id", "registration_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_event_attendances_congregation_id_check_in_time_id",
                table: "event_attendances",
                columns: new[] { "congregation_id", "check_in_time", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_congregation_id_start_date_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_event_registrations_congregation_id_registration_date_id",
                table: "event_registrations");

            migrationBuilder.DropIndex(
                name: "ix_event_attendances_congregation_id_check_in_time_id",
                table: "event_attendances");

            migrationBuilder.CreateIndex(
                name: "ix_events_congregation_id",
                table: "events",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_start_date_id",
                table: "events",
                columns: new[] { "start_date", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_event_registrations_congregation_id",
                table: "event_registrations",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_registrations_registration_date_id",
                table: "event_registrations",
                columns: new[] { "registration_date", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_event_attendances_check_in_time_id",
                table: "event_attendances",
                columns: new[] { "check_in_time", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_event_attendances_congregation_id",
                table: "event_attendances",
                column: "congregation_id");
        }
    }
}
