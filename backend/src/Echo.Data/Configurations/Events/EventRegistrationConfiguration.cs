using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Events;

public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(er => er.Member)
            .WithMany()
            .HasForeignKey(er => er.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(er => er.Event)
            .WithMany()
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(er => new { er.EventId, er.MemberId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // The list: newest registration first, cursor on registration_date + id.
        builder
            .HasIndex(er => new
            {
                er.CongregationId,
                er.RegistrationDate,
                er.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL");

        // One event's sign-ups in order, cursor on registration_date + id: a page of the event's list reads only its own
        // rows, in order, instead of sorting everyone who signed up.
        builder
            .HasIndex(er => new
            {
                er.EventId,
                er.RegistrationDate,
                er.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL");
    }
}
