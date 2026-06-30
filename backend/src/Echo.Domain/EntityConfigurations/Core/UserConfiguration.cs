using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class UserConfiguration : PrimaryEntityConfigurationBase<User>
{
    public override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.EmailAddress).IsUnique();
    }
}
