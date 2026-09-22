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

        builder
            .HasIndex(t => new { t.CollectionDate, t.Id })
            .HasFilter($"\"{nameof(Tithe.DeletedAt)}\" IS NULL");
    }
}
