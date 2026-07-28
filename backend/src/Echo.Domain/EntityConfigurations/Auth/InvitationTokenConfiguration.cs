using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Auth;

public class InvitationTokenEntityConfig : AuthEntityConfigurationBase<InvitationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<InvitationToken> builder)
    {
        builder.HasIndex(i => i.TokenHash).IsUnique();

        builder
            .HasOne(i => i.Congregation)
            .WithMany()
            .HasForeignKey(i => i.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
