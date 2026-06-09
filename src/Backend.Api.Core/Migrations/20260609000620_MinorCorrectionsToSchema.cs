using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Api.Core.Migrations
{
    /// <inheritdoc />
    public partial class MinorCorrectionsToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_MemberId",
                table: "AttendanceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_MemberId",
                table: "AttendanceRecords",
                column: "MemberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_Name",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_MemberId",
                table: "AttendanceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_MemberId",
                table: "AttendanceRecords",
                column: "MemberId");
        }
    }
}
