using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class InvitationTokenConfiguration : PrimaryEntityConfigurationBase<InvitationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<InvitationToken> builder)
    {
        builder.HasIndex(i => i.Token).IsUnique();
    }
}
