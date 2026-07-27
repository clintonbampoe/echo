using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SlightChangesToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "EmailVerificationTokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "EmailVerificationTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "EmailVerificationTokens");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "EmailVerificationTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
