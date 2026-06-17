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

        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                $"TRIM(CONCAT(\"{nameof(Member.LastName)}\", ' ', \"{nameof(Member.FirstName)}\", ' ', \"{nameof(Member.OtherNames)}\"))",
                stored: true
            );
    }
}
