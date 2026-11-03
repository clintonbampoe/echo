using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectTotalsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core cannot create a materialized view, so it is plain SQL. One row per project: how much was given and in how
            // many gifts, which the project list reads instead of adding up every gift. The unique index lets the refresh
            // run CONCURRENTLY, so readers are never blocked.
            migrationBuilder.Sql(
                """
                CREATE MATERIALIZED VIEW project_totals AS
                SELECT p.id AS project_id,
                       COALESCE(c.amount, 0) AS amount,
                       COALESCE(c.contributions, 0)::int AS contributions
                FROM projects p
                LEFT JOIN (
                    SELECT project_id, sum(amount) AS amount, count(*) AS contributions
                    FROM project_contributions
                    WHERE deleted_at IS NULL
                    GROUP BY project_id
                ) c ON c.project_id = p.id
                WHERE p.deleted_at IS NULL
                """
            );
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_project_totals_project_id ON project_totals (project_id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW project_totals");
        }
    }
}
