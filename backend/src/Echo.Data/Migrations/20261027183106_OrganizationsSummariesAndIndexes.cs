using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationsSummariesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_organizations_congregation_id",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "ix_organizations_name_id",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "ix_organization_members_congregation_id",
                table: "organization_members");

            migrationBuilder.DropIndex(
                name: "ix_organization_members_created_at_id",
                table: "organization_members");

            migrationBuilder.DropIndex(
                name: "ix_organization_members_organization_id",
                table: "organization_members");

            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "organizations",
                type: "text",
                nullable: false,
                defaultValue: "Other");

            migrationBuilder.AddColumn<string>(
                name: "meeting_day",
                table: "organizations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_congregation_id_created_at_id",
                table: "organization_members",
                columns: new[] { "congregation_id", "created_at", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_organization_id_role",
                table: "organization_members",
                columns: new[] { "organization_id", "role" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "member_id", "joined_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_organization_members_congregation_id_created_at_id",
                table: "organization_members");

            migrationBuilder.DropIndex(
                name: "ix_organization_members_organization_id_role",
                table: "organization_members");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "meeting_day",
                table: "organizations");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_congregation_id",
                table: "organizations",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_name_id",
                table: "organizations",
                columns: new[] { "name", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_congregation_id",
                table: "organization_members",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_created_at_id",
                table: "organization_members",
                columns: new[] { "created_at", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_organization_members_organization_id",
                table: "organization_members",
                column: "organization_id");
        }
    }
}
