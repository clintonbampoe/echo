using Echo.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);
        builder.HasIndex(o => new { o.CongregationId, o.Name }).IsUnique();

        builder.HasIndex(o => o.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        builder
            .HasIndex(p => new { p.Name, p.Id })
            .HasFilter($"\"{nameof(Organization.DeletedAt)}\" IS NULL");
    }
}
