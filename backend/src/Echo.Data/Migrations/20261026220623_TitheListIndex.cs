using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class TitheListIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tithes_collection_date_id",
                table: "tithes");

            migrationBuilder.DropIndex(
                name: "ix_tithes_congregation_id",
                table: "tithes");

            migrationBuilder.CreateIndex(
                name: "ix_tithes_congregation_id_collection_date_id",
                table: "tithes",
                columns: new[] { "congregation_id", "collection_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "amount", "member_id", "payment_method" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tithes_congregation_id_collection_date_id",
                table: "tithes");

            migrationBuilder.CreateIndex(
                name: "ix_tithes_collection_date_id",
                table: "tithes",
                columns: new[] { "collection_date", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tithes_congregation_id",
                table: "tithes",
                column: "congregation_id");
        }
    }
}
