using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class EventAttendanceConfiguration : PrimaryEntityConfigurationBase<EventAttendance>
{
    public override void ConfigureEntity(EntityTypeBuilder<EventAttendance> builder)
    {
        builder
            .HasOne(ea => ea.Member)
            .WithMany()
            .HasForeignKey(ea => ea.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(ea => ea.Event)
            .WithMany()
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ea => new { ea.EventId, ea.MemberId }).IsUnique();

        builder
            .HasIndex(ea => new { ea.CheckInTime, ea.Id })
            .HasFilter($"\"{nameof(EventAttendance.DeletedAt)}\" IS NULL");
    }
}
