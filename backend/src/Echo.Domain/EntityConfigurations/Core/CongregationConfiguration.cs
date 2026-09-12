using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class CongregationConfiguration : IEntityTypeConfiguration<Congregation>
{
    public void Configure(EntityTypeBuilder<Congregation> builder)
    {
        builder.HasKey(cong => cong.Id);
        builder.Property(cong => cong.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedOnAdd();
        builder.Property(cong => cong.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasIndex(cong => cong.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
        builder.HasIndex(cong => cong.Name);

        builder
            .HasIndex(cong => new { cong.CreatedAt, cong.Id })
            .HasFilter($"\"{nameof(Congregation.DeletedAt)}\" IS NULL");
    }
}
