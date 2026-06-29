using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class AttendanceContextConfiguration : ReferenceEntityConfigurationBase<AttendanceContext>
{
    public override void ConfigureEntity(EntityTypeBuilder<AttendanceContext> builder)
    {
        builder
            .HasOne(c => c.AttendanceType)
            .WithMany()
            .HasForeignKey(c => c.AttendanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.AttendanceTypeId, c.Name }).IsUnique();
    }
}
