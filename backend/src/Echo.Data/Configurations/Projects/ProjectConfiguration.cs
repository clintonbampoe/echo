using Echo.Domain.Congregations;
using Echo.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Projects;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
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
        builder.HasIndex(p => p.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        // Serves the list (oldest start first, cursor on start_date + id) and the summaries as
        // index-only scans.
        builder
            .HasIndex(p => new
            {
                p.CongregationId,
                p.StartDate,
                p.Id,
            })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(p => new
            {
                p.EndDate,
                p.Status,
                p.TargetAmount,
                p.CategoryId,
            });
    }
}
