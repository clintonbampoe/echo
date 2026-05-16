using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectContributionConfiguration : IEntityTypeConfiguration<ProjectContribution>
{
    public void Configure(EntityTypeBuilder<ProjectContribution> builder)
    {
        builder.HasKey(contribution => contribution.ContributionId);
        builder.HasAlternateKey(contribution => contribution.UniqueId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(contribution => contribution.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
