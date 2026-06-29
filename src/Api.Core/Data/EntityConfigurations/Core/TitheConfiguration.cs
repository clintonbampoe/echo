using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class TitheConfiguration : PrimaryEntityConfigurationBase<Tithe>
{
    public override void ConfigureEntity(EntityTypeBuilder<Tithe> builder)
    {
        builder
            .HasOne(t => t.Member)
            .WithMany()
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.MemberId);
    }
}
