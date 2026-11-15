using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexOnPersonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_people_name",
                table: "people");

            migrationBuilder.CreateIndex(
                name: "ix_people_congregation_id_name",
                table: "people",
                columns: new[] { "congregation_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_people_name",
                table: "people",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_people_congregation_id_name",
                table: "people");

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
    }
}
