using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class AssetCategoryConfiguration : CongregationEntityConfigurationBase<AssetCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasKey(assetCategory => assetCategory.Id);
    }
}
