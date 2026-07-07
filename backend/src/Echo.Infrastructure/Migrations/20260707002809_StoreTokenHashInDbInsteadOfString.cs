using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreTokenHashInDbInsteadOfString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerificationTokens_Users_UserId",
                table: "EmailVerificationTokens");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "EmailVerificationTokens",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_EmailVerificationTokens_Token",
                table: "EmailVerificationTokens",
                newName: "IX_EmailVerificationTokens_TokenHash");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerificationTokens_Users_UserId",
                table: "EmailVerificationTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerificationTokens_Users_UserId",
                table: "EmailVerificationTokens");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "EmailVerificationTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens",
                newName: "IX_EmailVerificationTokens_Token");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerificationTokens_Users_UserId",
                table: "EmailVerificationTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
