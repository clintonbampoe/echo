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
