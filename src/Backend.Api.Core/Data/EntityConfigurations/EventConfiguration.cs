using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class EventConfiguration : CongregationEntityConfigurationBase<Event>
{
    public override void ConfigureEntity(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Title).IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
