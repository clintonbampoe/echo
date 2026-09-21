using Echo.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Auth;

public class EmailVerificationTokenConfigurations : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.CreatedAt);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasIndex(e => e.TokenHash);

        // this index is to filter the token objects down to only the few ones that match these conditions
        // 1. Is not used
        // 2. Is not invalidated
        // 3. is not deleted
        builder
            .HasIndex(t => new { t.UserId, t.CreatedAt })
            .HasFilter(
                "\"UsedAt\" IS NULL AND \"InvalidatedAt\" IS NULL AND \"DeletedAt\" IS NULL"
            );

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
