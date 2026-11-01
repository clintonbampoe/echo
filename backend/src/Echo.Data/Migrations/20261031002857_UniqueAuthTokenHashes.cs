using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAuthTokenHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_password_verification_tokens_token_hash",
                table: "password_verification_tokens");

            migrationBuilder.DropIndex(
                name: "ix_email_verification_tokens_token_hash",
                table: "email_verification_tokens");

            migrationBuilder.CreateIndex(
                name: "ix_password_verification_tokens_token_hash",
                table: "password_verification_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_token_hash",
                table: "email_verification_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_password_verification_tokens_token_hash",
                table: "password_verification_tokens");

            migrationBuilder.DropIndex(
                name: "ix_email_verification_tokens_token_hash",
                table: "email_verification_tokens");

            migrationBuilder.CreateIndex(
                name: "ix_password_verification_tokens_token_hash",
                table: "password_verification_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_token_hash",
                table: "email_verification_tokens",
                column: "token_hash");
        }
    }
}
