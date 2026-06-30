using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class ProjectConfiguration : PrimaryEntityConfigurationBase<Project>
{
    public override void ConfigureEntity(EntityTypeBuilder<Project> builder)
    {
        builder
            .HasOne(p => p.Manager)
            .WithMany()
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.ManagerId);
    }
}
