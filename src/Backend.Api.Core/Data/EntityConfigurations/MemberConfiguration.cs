using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

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
