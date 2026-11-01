using Echo.Domain.Congregations;
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
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(m => m.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Serves the summary, the join-date window and the new-members chart as index-only scans.
        // Not partial on joined_date: members without one are part of every window.
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
