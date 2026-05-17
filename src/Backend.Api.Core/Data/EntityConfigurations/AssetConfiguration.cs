using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(asset => asset.AssetId);
        builder.HasAlternateKey(asset => asset.UniqueId);

        builder.HasOne<AssetCategory>()
            .WithMany()
            .HasForeignKey(asset => asset.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
