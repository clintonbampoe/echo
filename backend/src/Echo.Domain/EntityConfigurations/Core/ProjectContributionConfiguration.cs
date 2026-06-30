using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

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
