using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class EventRegistrationConfiguration : PrimaryEntityConfigurationBase<EventRegistration>
{
    public override void ConfigureEntity(EntityTypeBuilder<EventRegistration> builder)
    {
        builder
            .HasOne(er => er.Member)
            .WithMany()
            .HasForeignKey(er => er.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(er => er.Event)
            .WithMany()
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(er => new { er.EventId, er.MemberId }).IsUnique();
    }
}
