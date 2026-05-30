using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(at => at.Id);

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(at => at.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(at => at.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
