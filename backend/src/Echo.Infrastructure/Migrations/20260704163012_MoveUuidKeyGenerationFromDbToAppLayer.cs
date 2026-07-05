using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveUuidKeyGenerationFromDbToAppLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetCategories_Congregations_CongregationId",
                table: "AssetCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceContexts_Congregations_CongregationId",
                table: "AttendanceContexts");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceTypes_Congregations_CongregationId",
                table: "AttendanceTypes");

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
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "CongregationId",
                table: "Congregations",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Tithes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Projects",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProjectContributions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PasswordVerificationTokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Organizations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "OrganizationMembers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Members",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InvitationTokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Events",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventRegistrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CheckInTime",
                table: "EventRegistrations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventAttendances",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EmailVerificationTokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GpsAddress",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrgType",
                table: "Congregations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalAddress",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Congregations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Town",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Congregations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Assets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetCategories_Congregations_CongregationId",
                table: "AssetCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceContexts_Congregations_CongregationId",
                table: "AttendanceContexts",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceTypes_Congregations_CongregationId",
                table: "AttendanceTypes",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendances_Congregations_CongregationId",
                table: "EventAttendances",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Congregations_CongregationId",
                table: "EventRegistrations",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Congregations_CongregationId",
                table: "Events",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Congregations_CongregationId",
                table: "Members",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_Congregations_CongregationId",
                table: "OrganizationMembers",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Congregations_CongregationId",
                table: "Organizations",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectCategories_Congregations_CongregationId",
                table: "ProjectCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectContributions_Congregations_CongregationId",
                table: "ProjectContributions",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Congregations_CongregationId",
                table: "Projects",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tithes_Congregations_CongregationId",
                table: "Tithes",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionCategories_Congregations_CongregationId",
                table: "TransactionCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Congregations_CongregationId",
                table: "Transactions",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetCategories_Congregations_CongregationId",
                table: "AssetCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceContexts_Congregations_CongregationId",
                table: "AttendanceContexts");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceTypes_Congregations_CongregationId",
                table: "AttendanceTypes");

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
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "GpsAddress",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "OrgType",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "PostalAddress",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "Town",
                table: "Congregations");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Congregations");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Congregations",
                newName: "CongregationId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Tithes",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProjectContributions",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PasswordVerificationTokens",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Organizations",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "OrganizationMembers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Members",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InvitationTokens",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Events",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventRegistrations",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventAttendances",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EmailVerificationTokens",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Assets",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetCategories_Congregations_CongregationId",
                table: "AssetCategories",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Congregations_CongregationId",
                table: "Assets",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceContexts_Congregations_CongregationId",
                table: "AttendanceContexts",
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
                name: "FK_AttendanceTypes_Congregations_CongregationId",
                table: "AttendanceTypes",
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
                name: "FK_InvitationTokens_Congregations_CongregationId",
                table: "InvitationTokens",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Congregations_CongregationId",
                table: "Users",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "CongregationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
