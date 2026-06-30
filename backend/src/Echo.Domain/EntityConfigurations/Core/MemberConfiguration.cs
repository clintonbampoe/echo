using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class MemberConfiguration : PrimaryEntityConfigurationBase<Member>
{
    public override void ConfigureEntity(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.FirstName).IsRequired();
        builder.Property(m => m.LastName).IsRequired();

        // Using immutable string concatenation (||) and COALESCE to handle nulls safely
        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                $"TRIM(COALESCE(\"{nameof(Member.LastName)}\", '') || ' ' || COALESCE(\"{nameof(Member.FirstName)}\", '') || ' ' || COALESCE(\"{nameof(Member.OtherNames)}\", ''))",
                stored: true
            );
    }
}
