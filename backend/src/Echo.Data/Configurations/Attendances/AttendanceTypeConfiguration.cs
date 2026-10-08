using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class AttendanceTypeConfiguration : IEntityTypeConfiguration<AttendanceType>
{
    public void Configure(EntityTypeBuilder<AttendanceType> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(s => s.Congregation)
            .WithMany()
            .HasForeignKey(s => s.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.CongregationId);
        builder.HasIndex(s => new { s.CongregationId, s.Name }).IsUnique();
        builder.HasIndex(s => s.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
