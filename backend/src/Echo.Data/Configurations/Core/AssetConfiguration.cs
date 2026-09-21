using Echo.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

        builder
            .HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CategoryId);
        builder.HasIndex(a => new { a.CongregationId, a.Name }).IsUnique();
        builder.HasIndex(a => a.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        builder
            .HasIndex(a => new { a.Name, a.Id })
            .HasFilter($"\"{nameof(Asset.DeletedAt)}\" IS NULL");
    }
}
