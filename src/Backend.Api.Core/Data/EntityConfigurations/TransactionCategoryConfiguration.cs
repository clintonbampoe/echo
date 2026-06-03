using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class TransactionCategoryConfiguration : CongregationEntityConfigurationBase<TransactionCategory>
{
    public override void ConfigureEntity(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.HasKey(cat => cat.Id);
    }
}
