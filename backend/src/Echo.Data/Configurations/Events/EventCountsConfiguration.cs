using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Events;

public class EventCountsConfiguration : IEntityTypeConfiguration<EventCounts>
{
    public const string ViewName = "event_counts";

    public void Configure(EntityTypeBuilder<EventCounts> builder)
    {
        // Mapped, not created: EF Core cannot create a materialized view. The migration that adds
        // it writes the SQL.
        builder.HasNoKey().ToView(ViewName);
    }
}
