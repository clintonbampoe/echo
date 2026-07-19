using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Auth;

public class EmailVerificationTokenEntityConfig
    : AuthEntityConfigurationBase<EmailVerificationToken>
{
    public override void ConfigureEntity(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasIndex(e => e.TokenHash);

        // this index is to filter the token objects down to only the few ones that match these conditions
        // 1. Is not used
        // 2. Is not invalidated
        // 3. is not deleted
        builder.HasIndex(t => new { t.UserId, t.CreatedAt })
            .HasFilter("\"UsedAt\" IS NULL AND \"InvalidatedAt\" IS NULL AND \"DeletedAt\" IS NULL");

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
