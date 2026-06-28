using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

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
