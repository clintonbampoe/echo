using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class ProjectCategoryConfiguration : ReferenceEntityConfigurationBase<ProjectCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<ProjectCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
