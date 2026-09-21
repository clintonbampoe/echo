using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorProjectStructures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DeletedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DeletedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_DeletedAt",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_Tithes_DeletedAt",
                table: "Tithes");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DeletedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectContributions_DeletedAt",
                table: "ProjectContributions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectCategories_DeletedAt",
                table: "ProjectCategories");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_DeletedAt",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_DeletedAt",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_Members_DeletedAt",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Events_DeletedAt",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_DeletedAt",
                table: "EventRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendances_DeletedAt",
                table: "EventAttendances");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceTypes_DeletedAt",
                table: "AttendanceTypes");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_DeletedAt",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceContexts_DeletedAt",
                table: "AttendanceContexts");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DeletedAt",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AssetCategories_DeletedAt",
                table: "AssetCategories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAt",
                table: "Users",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DeletedAt",
                table: "Transactions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_DeletedAt",
                table: "TransactionCategories",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tithes_DeletedAt",
                table: "Tithes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DeletedAt",
                table: "Projects",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectContributions_DeletedAt",
                table: "ProjectContributions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategories_DeletedAt",
                table: "ProjectCategories",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DeletedAt",
                table: "Organizations",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_DeletedAt",
                table: "OrganizationMembers",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Members_DeletedAt",
                table: "Members",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_DeletedAt",
                table: "Events",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_DeletedAt",
                table: "EventRegistrations",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_DeletedAt",
                table: "EventAttendances",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceTypes_DeletedAt",
                table: "AttendanceTypes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_DeletedAt",
                table: "AttendanceRecords",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceContexts_DeletedAt",
                table: "AttendanceContexts",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DeletedAt",
                table: "Assets",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_DeletedAt",
                table: "AssetCategories",
                column: "DeletedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
