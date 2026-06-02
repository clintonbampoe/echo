using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectContributionConfiguration : CongregationEntityConfigurationBase<ProjectContribution>
{
    public override void ConfigureEntity(EntityTypeBuilder<ProjectContribution> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.HasOne(pc => pc.Project)
            .WithMany()
            .HasForeignKey(contribution => contribution.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
