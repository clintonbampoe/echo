using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class AssetConfiguration : CongregationEntityConfigurationBase<Asset>
{
    public override void ConfigureEntity(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(asset => asset.Id);

        builder.HasOne(ass => ass.Category)
            .WithMany()
            .HasForeignKey(asset => asset.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
