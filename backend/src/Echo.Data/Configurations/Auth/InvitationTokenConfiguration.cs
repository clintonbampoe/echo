using Echo.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Auth;

public class InvitationTokenConfiguration : IEntityTypeConfiguration<InvitationToken>
{
    public void Configure(EntityTypeBuilder<InvitationToken> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.CreatedAt);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.Property(i => i.EncryptedCode).HasMaxLength(200);

        // The list (newest first) and the summary counts read only these columns, so both are
        // index-only scans.
        builder
            .HasIndex(i => new { i.CongregationId, i.CreatedAt, i.Id })
            .IsDescending(false, true, false)
            .IncludeProperties(i => new { i.IsRevoked, i.ExpiresAt, i.MaxUses, i.UseCount })
            .HasFilter("deleted_at IS NULL");
        builder.Property(i => i.Label).HasMaxLength(100);
        builder.Property(i => i.UseCount).HasDefaultValue(0);

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
