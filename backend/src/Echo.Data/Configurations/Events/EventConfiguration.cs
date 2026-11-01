using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Events;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(e => new { e.CongregationId, e.Name }).IsUnique();

        builder
            .HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");

        // Serves the list in either direction (cursor on start_date + id) and the summaries.
        builder
            .HasIndex(e => new
            {
                e.CongregationId,
                e.StartDate,
                e.Id,
            })
            .HasFilter("deleted_at IS NULL")
            .IncludeProperties(e => new { e.OrganizationId, e.OrganizerId });
    }
}
