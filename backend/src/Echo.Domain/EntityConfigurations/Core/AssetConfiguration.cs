using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

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
