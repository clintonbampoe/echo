using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class UserConfiguration : PrimaryEntityConfigurationBase<User>
{
    public override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.EmailAddress).IsUnique().HasFilter("\"DeletedAt\" IS NULL");

        // Using immutable string concatenation (||) and COALESCE to handle nulls safely
        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                $"TRIM(COALESCE(\"{nameof(User.LastName)}\", '') || ' ' || COALESCE(\"{nameof(User.FirstName)}\", '') || ' ' || COALESCE(\"{nameof(User.OtherNames)}\", ''))",
                stored: true
            );

        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
