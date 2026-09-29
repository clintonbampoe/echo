using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.EmailAddress).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        // Using immutable string concatenation (||) and COALESCE to handle nulls safely
        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                $"TRIM(COALESCE(\"{nameof(User.LastName)}\", '') || ' ' || COALESCE(\"{nameof(User.FirstName)}\", '') || ' ' || COALESCE(\"{nameof(User.OtherNames)}\", ''))",
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

        builder
            .HasIndex(u => new { u.Name, u.Id })
            .HasFilter($"\"{nameof(User.DeletedAt)}\" IS NULL");

        builder
            .HasIndex(u => new { u.EmailAddress, u.Id })
            .HasFilter($"\"{nameof(User.DeletedAt)}\" IS NULL");
    }
}
