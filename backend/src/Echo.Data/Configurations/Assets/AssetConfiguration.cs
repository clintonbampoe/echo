using Echo.Domain.Assets;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Assets;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CategoryId);
        builder.HasIndex(a => new { a.CongregationId, a.Name }).IsUnique();
        builder.HasIndex(a => a.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        // The summaries read a congregation's assets by purchase date, so everything they sum or group
        // by is in the index and they never touch the table.
        builder
            .HasIndex(a => new { a.CongregationId, a.PurchaseDate })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(a => new
            {
                a.CategoryId,
                a.Status,
                a.PurchaseCost,
                a.CurrentValue,
            });

        // The lost-per-month chart: only liquidated assets have a date.
        builder
            .HasIndex(a => new { a.CongregationId, a.LiquidatedOn })
            .HasFilter("deleted_at IS NULL AND liquidated_on IS NOT NULL")
            .IncludeProperties(a => a.PurchaseCost);
    }
}
