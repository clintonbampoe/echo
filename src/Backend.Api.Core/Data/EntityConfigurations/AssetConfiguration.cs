using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

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
