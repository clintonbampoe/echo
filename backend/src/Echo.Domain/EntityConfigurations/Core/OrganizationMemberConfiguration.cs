using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class OrganizationMemberConfiguration : PrimaryEntityConfigurationBase<OrganizationMember>
{
    public override void ConfigureEntity(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder
            .HasOne(om => om.Member)
            .WithMany()
            .HasForeignKey(om => om.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(om => om.Organization)
            .WithMany()
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(om => new { om.MemberId, om.OrganizationId }).IsUnique();
    }
}
