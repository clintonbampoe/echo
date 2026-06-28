using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class ProjectCategoryConfiguration : ReferenceEntityConfigurationBase<ProjectCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<ProjectCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
