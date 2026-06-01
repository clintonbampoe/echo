using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectCategoryConfiguration : CongregationEntityConfigurationBase<ProjectCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<ProjectCategory> builder)
    {
        builder.HasKey(category => category.Id);
        builder.HasIndex(category => category.Name).IsUnique();
    }
}
