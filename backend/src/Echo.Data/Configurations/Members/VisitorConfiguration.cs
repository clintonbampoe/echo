using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Members;

public class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
{
    public void Configure(EntityTypeBuilder<Visitor> builder)
    {
        builder.HasKey(v => v.PersonId);

        builder.Property(v => v.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(v => v.Person)
            .WithOne()
            .HasForeignKey<Visitor>(v => v.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(v => v.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(v => v.InvitedBy)
            .WithMany()
            .HasForeignKey(v => v.InvitedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.Address).HasMaxLength(255);

        // The default list and the new-visitor count window by created_at and split on converted_at.
        builder
            .HasIndex(v => new { v.CongregationId, v.CreatedAt })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(v => new { v.ConvertedAt, v.Gender });

        // The converted view and count window by converted_at.
        builder
            .HasIndex(v => new { v.CongregationId, v.ConvertedAt })
            .HasFilter("deleted_at IS NULL AND converted_at IS NOT NULL");
    }
}
