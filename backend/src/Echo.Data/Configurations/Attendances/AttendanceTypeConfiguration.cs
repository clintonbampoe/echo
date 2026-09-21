using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class AttendanceTypeConfiguration : IEntityTypeConfiguration<AttendanceType>
{
    public void Configure(EntityTypeBuilder<AttendanceType> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);
        builder.HasIndex(t => new { t.CongregationId, t.Name }).IsUnique();

        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
