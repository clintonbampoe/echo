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
            .HasOne(a => a.Person)
            .WithMany()
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(a => a.AttendanceType)
            .WithMany()
            .HasForeignKey(a => a.AttendanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate check-ins for the same person on the same date and service
        builder
            .HasIndex(a => new
            {
                a.CongregationId,
                a.PersonId,
                a.AttendanceTypeId,
                a.Date,
            })
            .IsUnique()
            .HasFilter($"\"{nameof(Attendance.DeletedAt)}\" IS NULL");

        // For fetching all attendance for a given service type + date (the implicit session)
        builder.HasIndex(a => new
        {
            a.CongregationId,
            a.AttendanceTypeId,
            a.Date,
        });

        // For fetching all attendance for a person (history)
        builder.HasIndex(a => new { a.CongregationId, a.PersonId });

        // For cursor pagination
        builder
            .HasIndex(a => new { a.Date, a.Id })
            .HasFilter($"\"{nameof(Attendance.DeletedAt)}\" IS NULL");
    }
}
