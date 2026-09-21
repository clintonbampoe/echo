using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

        builder.Property(m => m.FirstName).IsRequired();
        builder.Property(m => m.LastName).IsRequired();

        // Using immutable string concatenation (||) and COALESCE to handle nulls safely
        builder
            .Property(m => m.Name)
            .HasComputedColumnSql(
                $"TRIM(COALESCE(\"{nameof(Member.LastName)}\", '') || ' ' || COALESCE(\"{nameof(Member.FirstName)}\", '') || ' ' || COALESCE(\"{nameof(Member.OtherNames)}\", ''))",
                stored: true
            )
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        builder
            .HasIndex(m => new { m.Name, m.Id })
            .HasFilter($"\"{nameof(Member.DeletedAt)}\" IS NULL");
    }
}
