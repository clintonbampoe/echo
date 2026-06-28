using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class OrganizationConfiguration : PrimaryEntityConfigurationBase<Organization>
{
    public override void ConfigureEntity(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(o => new { o.CongregationId, o.Name }).IsUnique();
    }
}
