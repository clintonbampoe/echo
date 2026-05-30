using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(p => new { p.FirstName, p.LastName })
            .HasDatabaseName("FullName");

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(m => m.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(m => m.FirstName).IsRequired();
        builder.Property(m => m.LastName).IsRequired();
    }
}
