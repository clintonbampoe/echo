using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Auth;

public class RefreshTokenEntityConfig : AuthEntityConfigurationBase<RefreshToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasIndex(x => new { x.UserId, x.RevokedAt })
            .HasFilter("\"RevokedAt\" IS NULL AND \"DeletedAt\" IS NULL");

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
