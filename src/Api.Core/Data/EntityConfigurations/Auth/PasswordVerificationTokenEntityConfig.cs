using Api.Core.Data.EntityConfigurations.Auth.Base;
using Api.Core.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Auth;

public class PasswordVerificationTokenEntityConfig
    : AuthEntityConfigurationBase<PasswordVerificationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<PasswordVerificationToken> builder)
    {
        builder.HasIndex(e => e.Token);

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
