using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class EventAttendanceConfiguration : IEntityTypeConfiguration<EventAttendance>
{
    public void Configure(EntityTypeBuilder<EventAttendance> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasIndex(ea => new { ea.EventId, ea.MemberId }).IsUnique();

        builder
            .HasIndex(ea => new { ea.CheckInTime, ea.Id })
            .HasFilter($"\"{nameof(EventAttendance.DeletedAt)}\" IS NULL");

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

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
