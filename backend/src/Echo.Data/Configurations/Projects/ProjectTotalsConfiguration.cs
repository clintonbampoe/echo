using Echo.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Projects;

public class ProjectTotalsConfiguration : IEntityTypeConfiguration<ProjectTotals>
{
    public const string ViewName = "project_totals";

    public void Configure(EntityTypeBuilder<ProjectTotals> builder)
    {
        // Mapped, not created: EF Core cannot create a materialized view. The migration that adds
        // it writes the SQL.
        builder.HasNoKey().ToView(ViewName);
    }
}
