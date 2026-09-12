using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexesToSupportCursorPagination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Name_Id",
                table: "Users",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate_Id",
                table: "Transactions",
                columns: new[] { "TransactionDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tithes_CollectionDate_Id",
                table: "Tithes",
                columns: new[] { "CollectionDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate_Id",
                table: "Projects",
                columns: new[] { "StartDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectContributions_DateContributed_Id",
                table: "ProjectContributions",
                columns: new[] { "DateContributed", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name_Id",
                table: "Organizations",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_CreatedAt_Id",
                table: "OrganizationMembers",
                columns: new[] { "CreatedAt", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name_Id",
                table: "Members",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDate_Id",
                table: "Events",
                columns: new[] { "StartDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_RegistrationDate_Id",
                table: "EventRegistrations",
                columns: new[] { "RegistrationDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_CheckInTime_Id",
                table: "EventAttendances",
                columns: new[] { "CheckInTime", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Congregations_CreatedAt_Id",
                table: "Congregations",
                columns: new[] { "CreatedAt", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ForDate_Id",
                table: "AttendanceRecords",
                columns: new[] { "ForDate", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Name_Id",
                table: "Assets",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Name_Id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionDate_Id",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Tithes_CollectionDate_Id",
                table: "Tithes");

            migrationBuilder.DropIndex(
                name: "IX_Projects_StartDate_Id",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectContributions_DateContributed_Id",
                table: "ProjectContributions");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name_Id",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_CreatedAt_Id",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_Members_Name_Id",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Events_StartDate_Id",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_RegistrationDate_Id",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendances_CheckInTime_Id",
                table: "EventAttendances");

            migrationBuilder.DropIndex(
                name: "IX_Congregations_CreatedAt_Id",
                table: "Congregations");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ForDate_Id",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Name_Id",
                table: "Assets");
        }
    }
}
