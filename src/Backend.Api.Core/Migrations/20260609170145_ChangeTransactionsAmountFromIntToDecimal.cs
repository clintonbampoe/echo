using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Api.Core.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTransactionsAmountFromIntToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Decimal",
                table: "Tithes");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Tithes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Tithes");

            migrationBuilder.AddColumn<int>(
                name: "Decimal",
                table: "Tithes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
