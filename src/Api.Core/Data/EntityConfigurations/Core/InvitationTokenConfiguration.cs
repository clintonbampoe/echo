using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class InvitationTokenConfiguration : PrimaryEntityConfigurationBase<InvitationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<InvitationToken> builder)
    {
        builder.HasIndex(i => i.Token).IsUnique();
    }
}
