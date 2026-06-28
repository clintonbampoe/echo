using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class AttendanceTypeConfiguration : ReferenceEntityConfigurationBase<AttendanceType>
{
    public override void ConfigureEntity(EntityTypeBuilder<AttendanceType> builder)
    {
        builder.HasIndex(t => new { t.CongregationId, t.Name }).IsUnique();
    }
}
