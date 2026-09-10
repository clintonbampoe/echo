using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class OrganizationConfiguration : PrimaryEntityConfigurationBase<Organization>
{
    public override void ConfigureEntity(EntityTypeBuilder<Organization> builder)
    {
        builder.HasIndex(o => new { o.CongregationId, o.Name }).IsUnique();

        builder.HasIndex(o => o.Name).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
