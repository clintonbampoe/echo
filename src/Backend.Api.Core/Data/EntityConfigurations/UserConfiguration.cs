using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class UserConfiguration : CongregationEntityConfigurationBase<User>
{
    public override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
    }
}
