using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InvitationLimitsCodesAndPasswordChangedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_invitation_tokens_congregation_id_created_at",
                table: "invitation_tokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "password_changed_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            // Existing passwords were last set when the account was made, as far as anyone knows.
            migrationBuilder.Sql("UPDATE users SET password_changed_at = created_at;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                table: "invitation_tokens",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "encrypted_code",
                table: "invitation_tokens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_uses",
                table: "invitation_tokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitation_tokens_congregation_id_created_at_id",
                table: "invitation_tokens",
                columns: new[] { "congregation_id", "created_at", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "is_revoked", "expires_at", "max_uses", "use_count" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_invitation_tokens_congregation_id_created_at_id",
                table: "invitation_tokens");

            migrationBuilder.DropColumn(
                name: "password_changed_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "encrypted_code",
                table: "invitation_tokens");

            migrationBuilder.DropColumn(
                name: "max_uses",
                table: "invitation_tokens");

            // The old column cannot hold "never", so such invitations get a far-off date.
            migrationBuilder.Sql(
                "UPDATE invitation_tokens SET expires_at = '9999-12-31' WHERE expires_at IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                table: "invitation_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitation_tokens_congregation_id_created_at",
                table: "invitation_tokens",
                columns: new[] { "congregation_id", "created_at" });
        }
    }
}
