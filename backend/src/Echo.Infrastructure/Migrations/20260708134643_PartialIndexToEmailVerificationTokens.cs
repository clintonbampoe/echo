using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartialIndexToEmailVerificationTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationTokens_UserId",
                table: "EmailVerificationTokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvalidatedAt",
                table: "EmailVerificationTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_UserId_CreatedAt",
                table: "EmailVerificationTokens",
                columns: new[] { "UserId", "CreatedAt" },
                filter: "\"UsedAt\" IS NULL AND \"InvalidatedAt\" IS NULL AND \"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationTokens_UserId_CreatedAt",
                table: "EmailVerificationTokens");

            migrationBuilder.DropColumn(
                name: "InvalidatedAt",
                table: "EmailVerificationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_UserId",
                table: "EmailVerificationTokens",
                column: "UserId");
        }
    }
}
