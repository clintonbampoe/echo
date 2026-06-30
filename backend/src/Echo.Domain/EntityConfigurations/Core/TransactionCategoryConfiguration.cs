using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

public class TransactionCategoryConfiguration
    : ReferenceEntityConfigurationBase<TransactionCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
