using Echo.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Projects;

public class ProjectContributionConfiguration : IEntityTypeConfiguration<ProjectContribution>
{
    public void Configure(EntityTypeBuilder<ProjectContribution> builder)
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
            .HasOne(pc => pc.Project)
            .WithMany()
            .HasForeignKey(pc => pc.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => pc.ProjectId);

        builder
            .HasIndex(pc => new { pc.DateContributed, pc.Id })
            .HasFilter($"\"{nameof(ProjectContribution.DeletedAt)}\" IS NULL");
    }
}
