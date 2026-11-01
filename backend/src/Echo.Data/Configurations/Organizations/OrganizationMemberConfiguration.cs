using Echo.Domain.Congregations;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Organizations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
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
            .HasOne(om => om.Member)
            .WithMany()
            .HasForeignKey(om => om.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(om => om.Organization)
            .WithMany()
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // An organization's members: the counts, the leaders and the role filter read only this.
        builder
            .HasIndex(om => new { om.OrganizationId, om.Role })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(om => new { om.MemberId, om.JoinedAt });

        builder
            .HasIndex(om => new { om.MemberId, om.OrganizationId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // The congregation-wide list: newest membership first, cursor on created_at + id.
        builder
            .HasIndex(om => new
            {
                om.CongregationId,
                om.CreatedAt,
                om.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL");
    }
}
