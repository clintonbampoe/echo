using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261028010000_AddVisitorStatsIndex")]
    public partial class AddVisitorStatsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE INDEX ix_attendance_visitor_stats
                    ON attendance (person_id) INCLUDE (congregation_id, session_date)
                    WHERE deleted_at IS NULL AND person_kind_at_time = 'Visitor';
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_attendance_visitor_stats", table: "attendance");
        }
    }
}
