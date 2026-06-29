using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class AssetCategoryConfiguration : ReferenceEntityConfigurationBase<AssetCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
