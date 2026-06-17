using Backend.Api.Core.Data.EntityConfigurations.Base;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

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
