using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveInvitationTokenToAuthAndHashToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Users_CreatedById",
                table: "InvitationTokens");

            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_CreatedById",
                table: "InvitationTokens");

            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_DeletedAt",
                table: "InvitationTokens");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "InvitationTokens");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "InvitationTokens",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "InvitationTokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_Token",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_CreatedAt",
                table: "InvitationTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_CreatedByUserId",
                table: "InvitationTokens",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Users_CreatedByUserId",
                table: "InvitationTokens",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Users_CreatedByUserId",
                table: "InvitationTokens");

            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_CreatedAt",
                table: "InvitationTokens");

            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_CreatedByUserId",
                table: "InvitationTokens");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "InvitationTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "InvitationTokens",
                newName: "ExpiryDate");

            migrationBuilder.RenameIndex(
                name: "IX_InvitationTokens_TokenHash",
                table: "InvitationTokens",
                newName: "IX_InvitationTokens_Token");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "InvitationTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_CreatedById",
                table: "InvitationTokens",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_DeletedAt",
                table: "InvitationTokens",
                column: "DeletedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Users_CreatedById",
                table: "InvitationTokens",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
