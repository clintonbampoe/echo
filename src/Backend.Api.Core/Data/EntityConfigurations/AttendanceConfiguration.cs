using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class AttendanceConfiguration : CongregationEntityConfigurationBase<AttendanceRecord>
{
    public override void ConfigureEntity(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(attendance => attendance.Id);

        builder.HasOne(mem => mem.Member)
            .WithMany()
            .HasForeignKey(at => at.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
