using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberTrendsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_members_congregation_id_joined_date",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_members_congregation_id_person_id",
                table: "members",
                columns: new[] { "congregation_id", "person_id" },
                filter: "deleted_at IS NULL AND joined_date IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "status", "gender", "date_of_birth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_members_congregation_id_person_id",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_members_congregation_id_joined_date",
                table: "members",
                columns: new[] { "congregation_id", "joined_date" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "status", "gender", "date_of_birth" });
        }
    }
}
