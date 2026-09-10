using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigramIndexesToNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendances_Events_EventId",
                table: "EventAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendances_Members_MemberId",
                table: "EventAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Events_EventId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Members_MemberId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectContributions_Projects_ProjectId",
                table: "ProjectContributions");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "AttendanceRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_Name",
                table: "TransactionCategories",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategories_Name",
                table: "ProjectCategories",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Congregations_Name",
                table: "Congregations",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceTypes_Name",
                table: "AttendanceTypes",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceContexts_Name",
                table: "AttendanceContexts",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CongregationId_Name",
                table: "Assets",
                columns: new[] { "CongregationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Name",
                table: "Assets",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_Name",
                table: "AssetCategories",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Events_EventId",
                table: "EventAttendances",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Members_MemberId",
                table: "EventAttendances",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Events_EventId",
                table: "EventRegistrations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Members_MemberId",
                table: "OrganizationMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                table: "OrganizationMembers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectContributions_Projects_ProjectId",
                table: "ProjectContributions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendances_Events_EventId",
                table: "EventAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendances_Members_MemberId",
                table: "EventAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Events_EventId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Members_MemberId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectContributions_Projects_ProjectId",
                table: "ProjectContributions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_Name",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Name",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectCategories_Name",
                table: "ProjectCategories");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Events_Name",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Congregations_Name",
                table: "Congregations");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceTypes_Name",
                table: "AttendanceTypes");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceContexts_Name",
                table: "AttendanceContexts");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CongregationId_Name",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Name",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AssetCategories_Name",
                table: "AssetCategories");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CheckInTime",
                table: "EventRegistrations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "AttendanceRecords",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Events_EventId",
                table: "EventAttendances",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Members_MemberId",
                table: "EventAttendances",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Events_EventId",
                table: "EventRegistrations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Members_MemberId",
                table: "OrganizationMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Organizations_OrganizationId",
                table: "OrganizationMembers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectContributions_Projects_ProjectId",
                table: "ProjectContributions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
