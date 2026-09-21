using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

        builder
            .HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(a => a.AttendanceContext)
            .WithMany()
            .HasForeignKey(a => a.AttendanceContextId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.MemberId);
        builder.HasIndex(a => a.AttendanceContextId);
        builder.HasIndex(a => a.ForDate);

        builder
            .HasIndex(a => new { a.ForDate, a.Id })
            .HasFilter($"\"{nameof(Attendance.DeletedAt)}\" IS NULL");
    }
}
