using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class TitheConfiguration : CongregationEntityConfigurationBase<Tithe>
{
    public override void ConfigureEntity(EntityTypeBuilder<Tithe> builder)
    {
        builder.HasKey(tithe => tithe.Id);

        builder.HasOne(tt => tt.Member)
            .WithMany()
            .HasForeignKey(tithe => tithe.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
