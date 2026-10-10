using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitors_Members_ConvertedToMemberPersonId",
                table: "Visitors");

            migrationBuilder.DropIndex(
                name: "IX_Visitors_ConvertedToMemberPersonId",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "ConvertedToMemberPersonId",
                table: "Visitors");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ConvertedAt",
                table: "Visitors",
                column: "ConvertedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visitors_ConvertedAt",
                table: "Visitors");

            migrationBuilder.AddColumn<Guid>(
                name: "ConvertedToMemberPersonId",
                table: "Visitors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ConvertedToMemberPersonId",
                table: "Visitors",
                column: "ConvertedToMemberPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visitors_Members_ConvertedToMemberPersonId",
                table: "Visitors",
                column: "ConvertedToMemberPersonId",
                principalTable: "Members",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
