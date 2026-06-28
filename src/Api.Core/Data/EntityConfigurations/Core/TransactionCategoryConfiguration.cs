using Api.Core.Data.EntityConfigurations.Core.Base;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations.Core;

public class TransactionCategoryConfiguration
    : ReferenceEntityConfigurationBase<TransactionCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
