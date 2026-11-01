using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectsSummariesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_projects_congregation_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_projects_start_date_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_project_contributions_congregation_id",
                table: "project_contributions");

            migrationBuilder.DropIndex(
                name: "ix_project_contributions_date_contributed_id",
                table: "project_contributions");

            migrationBuilder.DropIndex(
                name: "ix_project_contributions_project_id",
                table: "project_contributions");

            migrationBuilder.CreateIndex(
                name: "ix_projects_congregation_id_start_date_id",
                table: "projects",
                columns: new[] { "congregation_id", "start_date", "id" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "end_date", "status", "target_amount", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_project_contributions_congregation_id_date_contributed_id",
                table: "project_contributions",
                columns: new[] { "congregation_id", "date_contributed", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "project_id", "amount", "payment_method" });

            migrationBuilder.CreateIndex(
                name: "ix_project_contributions_project_id_date_contributed",
                table: "project_contributions",
                columns: new[] { "project_id", "date_contributed" },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "amount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_projects_congregation_id_start_date_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_project_contributions_congregation_id_date_contributed_id",
                table: "project_contributions");

            migrationBuilder.DropIndex(
                name: "ix_project_contributions_project_id_date_contributed",
                table: "project_contributions");

            migrationBuilder.CreateIndex(
                name: "ix_projects_congregation_id",
                table: "projects",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_start_date_id",
                table: "projects",
                columns: new[] { "start_date", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_project_contributions_congregation_id",
                table: "project_contributions",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_contributions_date_contributed_id",
                table: "project_contributions",
                columns: new[] { "date_contributed", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_project_contributions_project_id",
                table: "project_contributions",
                column: "project_id");
        }
    }
}
