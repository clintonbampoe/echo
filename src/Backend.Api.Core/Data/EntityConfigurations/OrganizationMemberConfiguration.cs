using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.HasKey(om => new { om.MemberId, om.OrganizationId });

        builder.HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(om => om.CongregationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(om => om.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
