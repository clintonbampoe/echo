using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Core;

public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
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
            .HasOne(er => er.Member)
            .WithMany()
            .HasForeignKey(er => er.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(er => er.Event)
            .WithMany()
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(er => new { er.EventId, er.MemberId }).IsUnique();

        builder
            .HasIndex(er => new { er.RegistrationDate, er.Id })
            .HasFilter($"\"{nameof(EventRegistration.DeletedAt)}\" IS NULL");
    }
}
