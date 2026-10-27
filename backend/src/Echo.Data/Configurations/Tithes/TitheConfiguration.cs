using Echo.Domain.Congregations;
using Echo.Domain.Tithes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Tithes;

public class TitheConfiguration : IEntityTypeConfiguration<Tithe>
{
    public void Configure(EntityTypeBuilder<Tithe> builder)
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
            .HasOne(t => t.Member)
            .WithMany()
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.MemberId);

        builder
            .HasIndex(t => new
            {
                t.MemberId,
                t.ForYear,
                t.ForMonth,
            })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Serves the list (newest first, cursor on collection_date + id) and the summary as index-only scans.
        builder
            .HasIndex(t => new
            {
                t.CongregationId,
                t.CollectionDate,
                t.Id,
            })
            .IsDescending(false, true, false)
            .IncludeProperties(t => new
            {
                t.Amount,
                t.MemberId,
                t.PaymentMethod,
            })
            .HasFilter("deleted_at IS NULL");

        builder
            .HasIndex(t => new
            {
                t.CongregationId,

                t.ForYear,
                t.ForMonth,
            })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(t => new { t.Amount, t.MemberId });
    }
}
