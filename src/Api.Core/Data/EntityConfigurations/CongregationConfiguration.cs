using Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class CongregationConfiguration : IEntityTypeConfiguration<Congregation>
{
    public void Configure(EntityTypeBuilder<Congregation> builder)
    {
        builder.HasKey(cong => cong.CongregationId);

        builder
            .Property(cong => cong.CongregationId)
            .HasDefaultValueSql("uuidv7()")
            .ValueGeneratedOnAdd();

        builder.Property(cong => cong.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}
