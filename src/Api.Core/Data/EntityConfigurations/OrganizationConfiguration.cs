using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class OrganizationConfiguration : PrimaryEntityConfigurationBase<Organization>
{
    public override void ConfigureEntity(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(o => new { o.CongregationId, o.Name }).IsUnique();
    }
}
