using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Members;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.PersonId);

        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(m => m.Person)
            .WithOne()
            .HasForeignKey<Member>(m => m.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(m => m.Congregation)
            .WithMany()
            .HasForeignKey(m => m.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.CongregationId);
        builder.HasIndex(m => m.Status);
    }
}
