using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Api.Core.Migrations
{
    /// <inheritdoc />
    public partial class ImplementEntityRelationshipsToCongregationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CongregationId",
                table: "Transactions",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_CongregationId",
                table: "TransactionCategories",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tithes_CongregationId",
                table: "Tithes",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CongregationId",
                table: "Projects",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectContributions_CongregationId",
                table: "ProjectContributions",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategories_CongregationId",
                table: "ProjectCategories",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CongregationId",
                table: "Organizations",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_CongregationId",
                table: "OrganizationMembers",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CongregationId",
                table: "Members",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CongregationId",
                table: "Events",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_CongregationId",
                table: "EventRegistrations",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_CongregationId",
                table: "EventAttendances",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CongregationId",
                table: "AttendanceRecords",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CongregationId",
                table: "Assets",
                column: "CongregationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Congregations_CongregationId",
                table: "EventAttendances",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Congregations_CongregationId",
                table: "EventRegistrations",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Congregations_CongregationId",
                table: "Events",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Congregations_CongregationId",
                table: "Members",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Congregations_CongregationId",
                table: "OrganizationMembers",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Congregations_CongregationId",
                table: "Organizations",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectCategories_Congregations_CongregationId",
                table: "ProjectCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectContributions_Congregations_CongregationId",
                table: "ProjectContributions",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Congregations_CongregationId",
                table: "Projects",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tithes_Congregations_CongregationId",
                table: "Tithes",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionCategories_Congregations_CongregationId",
                table: "TransactionCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Congregations_CongregationId",
                table: "Transactions",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendances_Congregations_CongregationId",
                table: "EventAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Congregations_CongregationId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Congregations_CongregationId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Congregations_CongregationId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Congregations_CongregationId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Congregations_CongregationId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectCategories_Congregations_CongregationId",
                table: "ProjectCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectContributions_Congregations_CongregationId",
                table: "ProjectContributions");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Congregations_CongregationId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tithes_Congregations_CongregationId",
                table: "Tithes");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionCategories_Congregations_CongregationId",
                table: "TransactionCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Congregations_CongregationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CongregationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_CongregationId",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_Tithes_CongregationId",
                table: "Tithes");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CongregationId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectContributions_CongregationId",
                table: "ProjectContributions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectCategories_CongregationId",
                table: "ProjectCategories");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CongregationId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_CongregationId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_Members_CongregationId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Events_CongregationId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_CongregationId",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendances_CongregationId",
                table: "EventAttendances");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_CongregationId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CongregationId",
                table: "Assets");
        }
    }
}
