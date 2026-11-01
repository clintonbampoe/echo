using Echo.Domain.Congregations;
using Echo.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Organizations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Also serves the list, which pages through a congregation's organizations by name.
        builder.HasIndex(o => new { o.CongregationId, o.Name }).IsUnique();

        builder.Property(o => o.Kind).HasDefaultValue(OrganizationType.Other);

        builder.HasIndex(o => o.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

    }
}
