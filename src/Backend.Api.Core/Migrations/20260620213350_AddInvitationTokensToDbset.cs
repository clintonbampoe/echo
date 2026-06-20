using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Api.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationTokensToDbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationToken_Congregations_CongregationId",
                table: "InvitationToken"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationToken_Users_CreatedById",
                table: "InvitationToken"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_InvitationToken", table: "InvitationToken");

            migrationBuilder.RenameTable(name: "InvitationToken", newName: "InvitationTokens");

            migrationBuilder.RenameIndex(
                name: "IX_InvitationToken_Token",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_Token"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationToken_DeletedAt",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_DeletedAt"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationToken_CreatedById",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_CreatedById"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationToken_CongregationId",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_CongregationId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvitationTokens",
                table: "InvitationTokens",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Users_CreatedById",
                table: "InvitationTokens",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Users_CreatedById",
                table: "InvitationTokens"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_InvitationTokens", table: "InvitationTokens");

            migrationBuilder.RenameTable(name: "InvitationTokens", newName: "InvitationToken");

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_Token",
                table: "InvitationToken",
                newName: "IX_InvitationToken_Token"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_DeletedAt",
                table: "InvitationToken",
                newName: "IX_InvitationToken_DeletedAt"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_CreatedById",
                table: "InvitationToken",
                newName: "IX_InvitationToken_CreatedById"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_CongregationId",
                table: "InvitationToken",
                newName: "IX_InvitationToken_CongregationId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvitationToken",
                table: "InvitationToken",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationToken_Congregations_CongregationId",
                table: "InvitationToken",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationToken_Users_CreatedById",
                table: "InvitationToken",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
