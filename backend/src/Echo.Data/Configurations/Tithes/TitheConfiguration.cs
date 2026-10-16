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
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

        builder
            .HasOne(t => t.Member)
            .WithMany()
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.MemberId);

        // A member has one tithe per month. Topping up is an update of that tithe, not a second row. Deleted
        // tithes do not count, so a mistaken one can be deleted and recorded again.
        builder
            .HasIndex(t => new
            {
                t.MemberId,
                t.ForYear,
                t.ForMonth,
            })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => new { t.CollectionDate, t.Id }).HasFilter("deleted_at IS NULL");
    }
}
