using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Members;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(p => p.Congregation)
            .WithMany()
            .HasForeignKey(p => p.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CongregationId);

        builder.Property(p => p.FirstName).IsRequired();
        builder.Property(p => p.LastName).IsRequired();

        builder
            .Property(p => p.Name)
            .HasComputedColumnSql(
                $"TRIM(COALESCE(\"{nameof(Person.LastName)}\", '') || ' ' || COALESCE(\"{nameof(Person.FirstName)}\", '') || ' ' || COALESCE(\"{nameof(Person.OtherNames)}\", ''))",
                stored: true
            )
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(p => p.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        builder
            .HasIndex(p => new { p.Name, p.Id })
            .HasFilter($"\"{nameof(Person.DeletedAt)}\" IS NULL");

        builder.HasIndex(p => new { p.CongregationId, p.Kind });
    }
}
