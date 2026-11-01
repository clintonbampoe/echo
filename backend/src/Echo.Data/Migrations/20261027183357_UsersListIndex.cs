using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersListIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_congregation_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_name_id",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ix_users_congregation_id_name_id",
                table: "users",
                columns: new[] { "congregation_id", "name", "id" },
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_congregation_id_name_id",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ix_users_congregation_id",
                table: "users",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_name_id",
                table: "users",
                columns: new[] { "name", "id" },
                filter: "deleted_at IS NULL");
        }
    }
}
