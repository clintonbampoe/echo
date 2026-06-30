using Echo.Domain.Entities.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Auth.Base;

public abstract class AuthEntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IAuthEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        ConfigureEntity(builder);
    }

    public virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder) { }
}
