using Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Base;

public abstract class ReferenceEntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IReferenceEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CongregationId);
        builder.HasIndex(e => e.DeletedAt);

        ConfigureEntity(builder);
    }

    public virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder) { }
}
