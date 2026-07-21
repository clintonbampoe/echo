using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordVerificationTokensTableColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "PasswordVerificationTokens");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "PasswordVerificationTokens",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordVerificationTokens_Token",
                table: "PasswordVerificationTokens",
                newName: "IX_PasswordVerificationTokens_TokenHash");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "PasswordVerificationTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "PasswordVerificationTokens");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "PasswordVerificationTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordVerificationTokens_TokenHash",
                table: "PasswordVerificationTokens",
                newName: "IX_PasswordVerificationTokens_Token");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "PasswordVerificationTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
