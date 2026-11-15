using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToPersonNameField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_people_name",
                table: "people");

            migrationBuilder.CreateIndex(
                name: "ix_people_name",
                table: "people",
                column: "name",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_people_name",
                table: "people");

            migrationBuilder.CreateIndex(
                name: "ix_people_name",
                table: "people",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
