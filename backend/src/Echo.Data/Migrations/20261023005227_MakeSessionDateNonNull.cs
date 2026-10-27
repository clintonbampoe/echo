using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeSessionDateNonNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    -- 0. Fill the column from the sessions
                    UPDATE attendance a
                    SET session_date = s.date
                    FROM sessions s
                    WHERE s.id = a.session_id AND a.session_date IS NULL;

                    -- 1. Add a check constraint as NOT VALID 
                    ALTER TABLE attendance 
                    ADD CONSTRAINT ck_attendance_session_date_not_null 
                    CHECK (session_date IS NOT NULL) NOT VALID;
            
                    -- 2. Validate the constraint 
                    ALTER TABLE attendance 
                    VALIDATE CONSTRAINT ck_attendance_session_date_not_null;
            
                    -- 3. Promote to a native NOT NULL column 
                    ALTER TABLE attendance 
                    ALTER COLUMN session_date SET NOT NULL;
            
                    -- 4. Clean up the temporary check constraint
                    ALTER TABLE attendance 
                    DROP CONSTRAINT ck_attendance_session_date_not_null;
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "session_date",
                table: "attendance",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date"
            );
        }
    }
}
