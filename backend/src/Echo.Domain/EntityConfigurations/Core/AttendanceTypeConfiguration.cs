using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class AttendanceTypeConfiguration : ReferenceEntityConfigurationBase<AttendanceType>
{
    public override void ConfigureEntity(EntityTypeBuilder<AttendanceType> builder)
    {
        builder.HasIndex(t => new { t.CongregationId, t.Name }).IsUnique();

        builder.HasIndex(m => m.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
