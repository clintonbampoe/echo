using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class AttendanceTypeConfiguration : ReferenceEntityConfigurationBase<AttendanceType>
{
    public override void ConfigureEntity(EntityTypeBuilder<AttendanceType> builder)
    {
        builder.HasIndex(t => new { t.CongregationId, t.Name }).IsUnique();
    }
}
