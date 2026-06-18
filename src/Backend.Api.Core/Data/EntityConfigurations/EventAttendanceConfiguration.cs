using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class EventAttendanceConfiguration : PrimaryEntityConfigurationBase<EventAttendance>
{
    public override void ConfigureEntity(EntityTypeBuilder<EventAttendance> builder)
    {
        builder
            .HasOne(ea => ea.Member)
            .WithMany()
            .HasForeignKey(ea => ea.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ea => ea.Event)
            .WithMany()
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ea => new { ea.EventId, ea.MemberId }).IsUnique();
    }
}
