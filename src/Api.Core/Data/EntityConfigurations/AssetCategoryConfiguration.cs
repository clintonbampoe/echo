using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class AssetCategoryConfiguration : ReferenceEntityConfigurationBase<AssetCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
