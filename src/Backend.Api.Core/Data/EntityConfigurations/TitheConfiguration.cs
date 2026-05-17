using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class TitheConfiguration : IEntityTypeConfiguration<Tithe>
{
    public void Configure(EntityTypeBuilder<Tithe> builder)
    {
        builder.HasKey(tithe => tithe.TitheId);
        builder.HasAlternateKey(tithe => tithe.UniqueId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(tithe => tithe.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
