using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class TitheConfiguration : IEntityTypeConfiguration<Tithe>
{
    public void Configure(EntityTypeBuilder<Tithe> builder)
    {
        builder.HasKey(tithe => tithe.Id);

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(tithe => tithe.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(tithe => tithe.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
