using Echo.Domain.Attendances;
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
            .HasOne(a => a.Congregation)
            .WithMany()
            .HasForeignKey(a => a.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // A person can only be marked present once per session. Blocks
        // duplicate QR scans, retries, and races. Filtered to live rows so
        // a soft-delete followed by a re-mark is allowed.
        builder
            .HasIndex(a => new { a.SessionId, a.PersonId })
            .IsUnique()
            .HasFilter($"\"{nameof(Attendance.DeletedAt)}\" IS NULL");

        // Summary queries ("how many visitors attended this period") now hit
        // this index directly instead of joining to People and reading the
        // current Kind.
        builder.HasIndex(a => new { a.CongregationId, a.PersonKindAtTime });

        builder.HasIndex(a => a.CongregationId);
        builder.HasIndex(a => a.PersonId);
        builder.HasIndex(a => a.SessionId);
    }
}
