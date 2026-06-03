using Backend.Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations.Base;

public abstract class CongregationEntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
     where TEntity : class, ICongregationEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasOne(ent => ent.Congregation)
            .WithMany()
            .HasForeignKey(entity => entity.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        ConfigureEntity(builder);
    }

    public virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder)
    {

    }
}
