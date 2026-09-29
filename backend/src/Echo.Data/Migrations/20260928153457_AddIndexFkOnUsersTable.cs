using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexFkOnUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress_Id",
                table: "Users",
                columns: new[] { "EmailAddress", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailAddress_Id",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
