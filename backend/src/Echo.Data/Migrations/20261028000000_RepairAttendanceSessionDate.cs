using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <summary>
    /// DenormalizeSessionDate was edited after it ran, so some databases hold session_date as
    /// timestamptz (with -infinity rows) while the model says date. Idempotent.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261028000000_RepairAttendanceSessionDate")]
    public partial class RepairAttendanceSessionDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    DO $$
                    BEGIN
                        IF (SELECT data_type FROM information_schema.columns
                            WHERE table_name = 'attendance' AND column_name = 'session_date')
                           <> 'date' THEN
                            UPDATE attendance a
                            SET session_date = s.date
                            FROM sessions s
                            WHERE s.id = a.session_id
                              AND a.session_date = '-infinity';

                            ALTER TABLE attendance
                            ALTER COLUMN session_date TYPE date USING (session_date AT TIME ZONE 'UTC')::date;
                        END IF;
                    END $$;
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
