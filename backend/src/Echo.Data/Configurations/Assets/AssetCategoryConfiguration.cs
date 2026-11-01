using Echo.Domain.Assets;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Assets;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Property(c => c.Icon).HasMaxLength(40);

        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
