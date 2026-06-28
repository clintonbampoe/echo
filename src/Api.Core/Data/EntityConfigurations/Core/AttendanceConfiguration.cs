using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class AttendanceConfiguration : PrimaryEntityConfigurationBase<Attendance>
{
    public override void ConfigureEntity(EntityTypeBuilder<Attendance> builder)
    {
        builder
            .HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(a => a.AttendanceContext)
            .WithMany()
            .HasForeignKey(a => a.AttendanceContextId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.MemberId);
        builder.HasIndex(a => a.AttendanceContextId);
        builder.HasIndex(a => a.ForDate);
    }
}
