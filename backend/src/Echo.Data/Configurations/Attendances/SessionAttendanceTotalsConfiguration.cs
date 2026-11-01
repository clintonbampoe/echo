using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Attendances;

public class SessionAttendanceTotalsConfiguration
    : IEntityTypeConfiguration<SessionAttendanceTotals>
{
    public const string ViewName = "attendance_per_session";

    public void Configure(EntityTypeBuilder<SessionAttendanceTotals> builder)
    {
        // Mapped, not created: EF Core cannot create a materialized view. The migration that adds
        // it writes the SQL.
        builder.HasNoKey().ToView(ViewName);
    }
}
