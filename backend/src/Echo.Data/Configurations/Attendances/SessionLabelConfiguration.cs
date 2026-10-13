using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class SessionLabelConfiguration : IEntityTypeConfiguration<SessionLabel>
{
    public void Configure(EntityTypeBuilder<SessionLabel> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(st => st.Congregation)
            .WithMany()
            .HasForeignKey(st => st.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(st => st.Name).IsRequired().HasMaxLength(100);

        // Kind is not user-editable and not shown in the UI. It exists so
        // cross-congregation analytics can group by category even when
        // congregations rename the display name.
        builder.Property(st => st.Category).IsRequired();

        builder.HasIndex(st => st.CongregationId);

        builder
            .HasIndex(st => new { st.CongregationId, st.Name })
            .HasFilter($"\"{nameof(SessionLabel.DeletedAt)}\" IS NULL");

        // SessionType names are unique per congregation
        builder.HasIndex(st => new { st.CongregationId, st.Name }).IsUnique();
    }
}
