using Echo.Domain.Congregations;
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
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(pc => pc.Project)
            .WithMany()
            .HasForeignKey(pc => pc.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // What a project has raised, and when: the totals per project and the monthly series read
        // only this index.
        builder
            .HasIndex(pc => new { pc.ProjectId, pc.DateContributed })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(pc => pc.Amount);

        // Serves the list (newest first, cursor on date + id) and the congregation-wide summaries.
        builder
            .HasIndex(pc => new
            {
                pc.CongregationId,
                pc.DateContributed,
                pc.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(pc => new
            {
                pc.ProjectId,
                pc.Amount,
                pc.PaymentMethod,
            });
    }
}
