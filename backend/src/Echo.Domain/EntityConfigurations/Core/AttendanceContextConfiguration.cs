using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

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
