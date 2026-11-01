using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(s => s.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(s => s.SessionLabel)
            .WithMany()
            .HasForeignKey(s => s.SessionLabelId)
            .OnDelete(DeleteBehavior.Restrict);

        // CreatedBy is nullable so historical imports and system-generated
        // sessions (recurring services, auto-scheduled events) don't need a user.
        builder
            .HasOne(s => s.Creator)
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.Name).HasMaxLength(200);
        builder.Property(s => s.Topic).HasMaxLength(500);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.Location).HasMaxLength(200);
        builder.Property(s => s.Status).IsRequired();

        // Sessions are almost always queried "for this congregation, on this
        // date" or "for this congregation, in this date range".
        builder.HasIndex(s => new { s.CongregationId, s.Date });
        builder.HasIndex(s => s.SessionLabelId);
    }
}
