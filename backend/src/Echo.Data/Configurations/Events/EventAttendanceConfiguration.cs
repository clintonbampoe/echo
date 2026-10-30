using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Events;

public class EventAttendanceConfiguration : IEntityTypeConfiguration<EventAttendance>
{
    public void Configure(EntityTypeBuilder<EventAttendance> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasIndex(ea => new { ea.EventId, ea.MemberId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // The list: latest check-in time first, cursor on check_in_time + id.
        builder
            .HasIndex(ea => new
            {
                ea.CongregationId,
                ea.CheckInTime,
                ea.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL");

        // One event's check-ins in order, cursor on check_in_time + id: a page of the event's list reads only its own
        // rows, in order, instead of sorting everyone who came.
        builder
            .HasIndex(ea => new
            {
                ea.EventId,
                ea.CheckInTime,
                ea.Id,
            })
            .IsDescending(false, true, false)
            .HasFilter("deleted_at IS NULL");

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
    }
}
