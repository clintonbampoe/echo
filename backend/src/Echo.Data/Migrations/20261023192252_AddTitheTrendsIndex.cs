using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTitheTrendsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_tithes_congregation_id_for_year_for_month",
                table: "tithes",
                columns: new[] { "congregation_id", "for_year", "for_month" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "amount", "member_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tithes_congregation_id_for_year_for_month",
                table: "tithes");
        }
    }
}
