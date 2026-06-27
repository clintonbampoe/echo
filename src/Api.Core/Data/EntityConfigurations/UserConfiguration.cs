using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class UserConfiguration : PrimaryEntityConfigurationBase<User>
{
    public override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.EmailAddress).IsUnique();
    }
}
