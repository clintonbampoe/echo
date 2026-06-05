using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class EventRegistrationConfiguration : CongregationEntityConfigurationBase<EventRegistration>
{
    public override void ConfigureEntity(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(eventReg => new { eventReg.EventId, eventReg.MemberId });

        builder.HasOne(eventReg => eventReg.Member)
            .WithMany()
            .HasForeignKey(ea => ea.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eventReg => eventReg.Event)
            .WithMany()
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
