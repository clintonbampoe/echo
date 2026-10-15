using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Members;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.PersonId);

        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(m => m.Person)
            .WithOne()
            .HasForeignKey<Member>(m => m.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(m => m.Congregation)
            .WithMany()
            .HasForeignKey(m => m.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite covering index optimizing dashboard analytics
        builder
            .HasIndex(m => new { m.CongregationId, m.JoinedDate })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(m => new
            {
                m.Status,
                m.Gender,
                m.DateOfBirth,
            });

        builder.HasIndex(m => m.Status);
    }
}
