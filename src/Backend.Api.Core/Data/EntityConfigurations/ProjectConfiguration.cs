using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectConfiguration : CongregationEntityConfigurationBase<Project>
{
    public override void ConfigureEntity(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.HasOne(pr => pr.Manager)
            .WithMany()
            .HasForeignKey(pr => pr.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(pr => pr.Category)
            .WithMany()
            .HasForeignKey(pr => pr.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
