using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class InvitationTokenConfiguration : PrimaryEntityConfigurationBase<InvitationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<InvitationToken> builder)
    {
        builder.HasIndex(i => i.Token).IsUnique();
    }
}
