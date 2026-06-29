using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

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
