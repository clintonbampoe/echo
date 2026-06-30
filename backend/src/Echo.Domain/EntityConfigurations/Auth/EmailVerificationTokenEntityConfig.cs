using Echo.Domain.Entities.Auth;
using Echo.Domain.EntityConfigurations.Auth.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Auth;

public class EmailVerificationTokenEntityConfig
    : AuthEntityConfigurationBase<EmailVerificationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasIndex(e => e.Token);

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
