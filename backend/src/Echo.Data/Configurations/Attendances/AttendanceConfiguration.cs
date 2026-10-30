using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(a => a.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(a => a.Session)
            .WithMany()
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Person)
            .WithMany()
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.Recorder)
            .WithMany()
            .HasForeignKey(a => a.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Snapshot of the person's role at the time of attendance. Required
        // because Person.Kind is a moving value and historical reports must
        // reflect what was true when the row was written.
        builder.Property(a => a.PersonKindAtTime).IsRequired();

        // One mark per person per session. Also answers "who was at this session", split by kind
        // and first visit, without touching the table.
        builder
            .HasIndex(a => new { a.SessionId, a.PersonId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(a => new { a.PersonKindAtTime, a.IsFirstVisit });

        // Serves the list (newest first, cursor on session_date + id) and the summary and chart
        // aggregates as index-only scans: everything they read is a key or an included column.
        builder
            .HasIndex(a => new
            {
                a.CongregationId,
                a.SessionDate,
                a.Id,
            })
            .IncludeProperties(a => new { a.PersonKindAtTime, a.IsFirstVisit })
            .HasFilter("deleted_at IS NULL");

        // A visitor who came again has a visit that was not their first. This small index lets the
        // visitor summary find them without counting every visit.
        builder
            .HasIndex(a => new { a.CongregationId, a.PersonId })
            .HasFilter(
                "deleted_at IS NULL AND person_kind_at_time = 'Visitor' AND NOT is_first_visit"
            );

        // Visit count and last visit for a page of visitors, as an index-only scan. A visitor who
        // converted has many member rows under the same person_id; this index leaves them out.
        builder
            .HasIndex(a => a.PersonId)
            .HasDatabaseName("ix_attendance_visitor_stats")
            .IncludeProperties(a => new { a.CongregationId, a.SessionDate })
            .HasFilter("deleted_at IS NULL AND person_kind_at_time = 'Visitor'");

        builder.HasIndex(a => a.PersonId);
        builder.HasIndex(a => a.SessionId);
    }
}
