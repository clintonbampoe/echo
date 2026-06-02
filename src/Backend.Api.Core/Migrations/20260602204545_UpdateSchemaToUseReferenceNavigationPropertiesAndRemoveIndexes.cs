using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Api.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaToUseReferenceNavigationPropertiesAndRemoveIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Members_ProjectManagerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_TransactionCategories_Name",
                table: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProjectCategories_Name",
                table: "ProjectCategories");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Events_Name",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "ProjectManagerId",
                table: "Projects",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects",
                newName: "IX_Projects_ManagerId");

            migrationBuilder.RenameColumn(
                name: "JoinedDate",
                table: "OrganizationMembers",
                newName: "JoinedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Assets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Members_ManagerId",
                table: "Projects",
                column: "ManagerId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Members_ManagerId",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Projects",
                newName: "ProjectManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ManagerId",
                table: "Projects",
                newName: "IX_Projects_ProjectManagerId");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "OrganizationMembers",
                newName: "JoinedDate");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Assets",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_Name",
                table: "TransactionCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategories_Name",
                table: "ProjectCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Members_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
