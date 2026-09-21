using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne(e => e.Congregation)
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CongregationId);

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

        builder
            .HasIndex(e => new { e.StartDate, e.Id })
            .HasFilter($"\"{nameof(Event.DeletedAt)}\" IS NULL");
    }
}
