using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetsSummariesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_assets_congregation_id",
                table: "assets");

            migrationBuilder.DropIndex(
                name: "ix_assets_name_id",
                table: "assets");

            migrationBuilder.DropIndex(
                name: "ix_asset_categories_congregation_id",
                table: "asset_categories");

            migrationBuilder.AddColumn<DateOnly>(
                name: "liquidated_on",
                table: "assets",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "asset_categories",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_assets_congregation_id_liquidated_on",
                table: "assets",
                columns: new[] { "congregation_id", "liquidated_on" },
                filter: "deleted_at IS NULL AND liquidated_on IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "purchase_cost" });

            migrationBuilder.CreateIndex(
                name: "ix_assets_congregation_id_purchase_date",
                table: "assets",
                columns: new[] { "congregation_id", "purchase_date" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "category_id", "status", "purchase_cost", "current_value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_assets_congregation_id_liquidated_on",
                table: "assets");

            migrationBuilder.DropIndex(
                name: "ix_assets_congregation_id_purchase_date",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "liquidated_on",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "icon",
                table: "asset_categories");

            migrationBuilder.CreateIndex(
                name: "ix_assets_congregation_id",
                table: "assets",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_name_id",
                table: "assets",
                columns: new[] { "name", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_congregation_id",
                table: "asset_categories",
                column: "congregation_id");
        }
    }
}
