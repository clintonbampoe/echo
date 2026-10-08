using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class FilterSoftDeletedFromMembershipUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_MemberId_OrganizationId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_EventId_MemberId",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendances_EventId_MemberId",
                table: "EventAttendances");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_MemberId_OrganizationId",
                table: "OrganizationMembers",
                columns: new[] { "MemberId", "OrganizationId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId_MemberId",
                table: "EventRegistrations",
                columns: new[] { "EventId", "MemberId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_EventId_MemberId",
                table: "EventAttendances",
                columns: new[] { "EventId", "MemberId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_MemberId_OrganizationId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_EventId_MemberId",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendances_EventId_MemberId",
                table: "EventAttendances");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_MemberId_OrganizationId",
                table: "OrganizationMembers",
                columns: new[] { "MemberId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId_MemberId",
                table: "EventRegistrations",
                columns: new[] { "EventId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_EventId_MemberId",
                table: "EventAttendances",
                columns: new[] { "EventId", "MemberId" },
                unique: true);
        }
    }
}
