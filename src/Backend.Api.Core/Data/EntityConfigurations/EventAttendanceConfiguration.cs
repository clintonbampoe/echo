using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class EventAttendanceConfiguration : CongregationEntityConfigurationBase<EventAttendance>
{
    public override void ConfigureEntity(EntityTypeBuilder<EventAttendance> builder)
    {
        builder.HasKey(ea => new { ea.EventId, ea.MemberId });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(ea => ea.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
