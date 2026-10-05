using Echo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Members;

public class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
{
    public void Configure(EntityTypeBuilder<Visitor> builder)
    {
        builder.HasKey(v => v.PersonId);

        builder.Property(v => v.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(v => v.Person)
            .WithOne()
            .HasForeignKey<Visitor>(v => v.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(v => v.Congregation)
            .WithMany()
            .HasForeignKey(v => v.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(v => v.ConvertedToMember)
            .WithMany()
            .HasForeignKey(v => v.ConvertedToMemberPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.CongregationId);
        builder.HasIndex(v => v.ConvertedToMemberPersonId);
    }
}
