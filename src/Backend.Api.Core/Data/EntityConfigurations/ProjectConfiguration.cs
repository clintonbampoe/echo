using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectConfiguration : CongregationEntityConfigurationBase<Project>
{
    public override void ConfigureEntity(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(pr => pr.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ProjectCategory>()
            .WithMany()
            .HasForeignKey(pr => pr.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
