using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class TransactionCategoryConfiguration
    : ReferenceEntityConfigurationBase<TransactionCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.HasIndex(c => new { c.CongregationId, c.Name }).IsUnique();
    }
}
