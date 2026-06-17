using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectContributionConfiguration : PrimaryEntityConfigurationBase<ProjectContribution>
{
    public override void ConfigureEntity(EntityTypeBuilder<ProjectContribution> builder)
    {
        builder
            .HasOne(pc => pc.Project)
            .WithMany()
            .HasForeignKey(pc => pc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pc => pc.ProjectId);
    }
}
