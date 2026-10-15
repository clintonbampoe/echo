using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.EmailAddress).IsUnique().HasFilter("deleted_at IS NULL");

        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                "TRIM(COALESCE(last_name, '') || ' ' || COALESCE(first_name, '') || ' ' || COALESCE(other_names, ''))",
                stored: true
            )
            .ValueGeneratedOnAddOrUpdate();

        builder
            .HasOne(u => u.Congregation)
            .WithMany()
            .HasForeignKey(u => u.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.CongregationId);

        builder.HasIndex(u => u.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        builder.HasIndex(u => new { u.Name, u.Id }).HasFilter("deleted_at IS NULL");

        builder.HasIndex(u => new { u.EmailAddress, u.Id }).HasFilter("deleted_at IS NULL");
    }
}
