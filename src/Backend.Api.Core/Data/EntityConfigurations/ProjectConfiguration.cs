using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(pr => pr.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

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
