using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceFirstVisitAndReadIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_first_visit",
                table: "attendance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Visibility now comes from the attendance row alone, so hide what a deleted session hid.
            migrationBuilder.Sql(
                @"
                    UPDATE attendance a
                    SET deleted_at = s.deleted_at
                    FROM sessions s
                    WHERE s.id = a.session_id AND s.deleted_at IS NOT NULL AND a.deleted_at IS NULL;
                "
            );

            // A visitor's first visit is their earliest live attendance as a visitor.
            migrationBuilder.Sql(
                @"
                    UPDATE attendance a
                    SET is_first_visit = true
                    FROM (
                        SELECT id,
                               row_number() OVER (PARTITION BY person_id ORDER BY session_date, id) AS rn
                        FROM attendance
                        WHERE person_kind_at_time = 'Visitor' AND deleted_at IS NULL
                    ) r
                    WHERE a.id = r.id AND r.rn = 1;
                "
            );

            migrationBuilder.DropIndex(
                name: "ix_attendance_congregation_id",
                table: "attendance");

            migrationBuilder.DropIndex(
                name: "ix_attendance_congregation_id_person_kind_at_time",
                table: "attendance");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_congregation_id_session_date_id",
                table: "attendance",
                columns: new[] { "congregation_id", "session_date", "id" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "person_kind_at_time", "is_first_visit" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_attendance_congregation_id_session_date_id",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "is_first_visit",
                table: "attendance");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_congregation_id",
                table: "attendance",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_congregation_id_person_kind_at_time",
                table: "attendance",
                columns: new[] { "congregation_id", "person_kind_at_time" });

        }
    }
}
