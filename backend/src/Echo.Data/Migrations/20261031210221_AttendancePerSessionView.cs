using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttendancePerSessionView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core cannot create a materialized view, so it is plain SQL. One row per session
            // that has check-ins; the totals the attendance and session summaries add up. The
            // unique index lets the refresh run CONCURRENTLY, so readers are never blocked.
            migrationBuilder.Sql(
                """
                CREATE MATERIALIZED VIEW attendance_per_session AS
                SELECT a.congregation_id,
                       a.session_id,
                       a.session_date,
                       s.session_label_id,
                       count(*)::int AS total,
                       (count(*) FILTER (WHERE a.person_kind_at_time = 'Member'))::int AS members,
                       (count(*) FILTER (WHERE a.person_kind_at_time = 'Visitor'))::int AS visitors,
                       (count(*) FILTER (WHERE a.is_first_visit))::int AS first_time
                FROM attendance a
                JOIN sessions s ON s.id = a.session_id
                WHERE a.deleted_at IS NULL
                GROUP BY a.congregation_id, a.session_id, a.session_date, s.session_label_id
                """
            );

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ix_attendance_per_session_session_id ON attendance_per_session (session_id)"
            );

            migrationBuilder.Sql(
                "CREATE INDEX ix_attendance_per_session_congregation_id_session_date ON attendance_per_session (congregation_id, session_date)"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW attendance_per_session");
        }
    }
}
