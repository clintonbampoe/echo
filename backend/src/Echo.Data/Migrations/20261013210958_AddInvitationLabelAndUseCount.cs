using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationLabelAndUseCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_CongregationId",
                table: "InvitationTokens");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "InvitationTokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UseCount",
                table: "InvitationTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_CongregationId_CreatedAt",
                table: "InvitationTokens",
                columns: new[] { "CongregationId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvitationTokens_CongregationId_CreatedAt",
                table: "InvitationTokens");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "InvitationTokens");

            migrationBuilder.DropColumn(
                name: "UseCount",
                table: "InvitationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_CongregationId",
                table: "InvitationTokens",
                column: "CongregationId");
        }
    }
}
