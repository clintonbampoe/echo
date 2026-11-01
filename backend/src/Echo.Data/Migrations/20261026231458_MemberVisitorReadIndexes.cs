using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class MemberVisitorReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_visitors_congregation_id",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_visitors_converted_at",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_people_congregation_id",
                table: "people");

            migrationBuilder.DropIndex(
                name: "ix_people_name_id",
                table: "people");

            migrationBuilder.DropIndex(
                name: "ix_members_congregation_id_person_id",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_visitors_congregation_id_converted_at",
                table: "visitors",
                columns: new[] { "congregation_id", "converted_at" },
                filter: "deleted_at IS NULL AND converted_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_visitors_congregation_id_created_at",
                table: "visitors",
                columns: new[] { "congregation_id", "created_at" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "converted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_people_congregation_id_name_id",
                table: "people",
                columns: new[] { "congregation_id", "name", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_members_congregation_id_joined_date",
                table: "members",
                columns: new[] { "congregation_id", "joined_date" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "status", "gender", "date_of_birth" });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_congregation_id_person_id",
                table: "attendance",
                columns: new[] { "congregation_id", "person_id" },
                filter: "deleted_at IS NULL AND person_kind_at_time = 'Visitor' AND NOT is_first_visit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_visitors_congregation_id_converted_at",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_visitors_congregation_id_created_at",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_people_congregation_id_name_id",
                table: "people");

            migrationBuilder.DropIndex(
                name: "ix_members_congregation_id_joined_date",
                table: "members");

            migrationBuilder.DropIndex(
                name: "ix_attendance_congregation_id_person_id",
                table: "attendance");

            migrationBuilder.CreateIndex(
                name: "ix_visitors_congregation_id",
                table: "visitors",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_visitors_converted_at",
                table: "visitors",
                column: "converted_at");

            migrationBuilder.CreateIndex(
                name: "ix_people_congregation_id",
                table: "people",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_people_name_id",
                table: "people",
                columns: new[] { "name", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_members_congregation_id_person_id",
                table: "members",
                columns: new[] { "congregation_id", "person_id" },
                filter: "deleted_at IS NULL AND joined_date IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "status", "gender", "date_of_birth" });
        }
    }
}
