using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
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
            .HasOne(om => om.Member)
            .WithMany()
            .HasForeignKey(om => om.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(om => om.Organization)
            .WithMany()
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(om => new { om.MemberId, om.OrganizationId }).IsUnique();

        builder
            .HasIndex(om => new { om.CreatedAt, om.Id })
            .HasFilter($"\"{nameof(Project.DeletedAt)}\" IS NULL");
    }
}
