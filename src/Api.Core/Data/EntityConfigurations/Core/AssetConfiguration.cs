using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class AssetConfiguration : PrimaryEntityConfigurationBase<Asset>
{
    public override void ConfigureEntity(EntityTypeBuilder<Asset> builder)
    {
        builder
            .HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CategoryId);
    }
}
