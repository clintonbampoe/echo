using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Congregations;

public class CongregationConfiguration : IEntityTypeConfiguration<Congregation>
{
    public void Configure(EntityTypeBuilder<Congregation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedOnAdd();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasIndex(c => c.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
        builder.HasIndex(c => c.Name);

        builder
            .HasIndex(c => new { c.CreatedAt, c.Id })
            .HasFilter($"\"{nameof(Congregation.DeletedAt)}\" IS NULL");
    }
}
