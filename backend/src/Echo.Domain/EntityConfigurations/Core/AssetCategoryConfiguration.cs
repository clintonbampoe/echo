using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class AssetCategoryConfiguration : ReferenceEntityConfigurationBase<AssetCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
