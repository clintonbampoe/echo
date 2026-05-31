using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class FinancialTransactionConfiguration : CongregationEntityConfigurationBase<FinancialTransaction>
{
    public override void ConfigureEntity(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.HasOne<TransactionCategory>()
            .WithMany()
            .HasForeignKey(tr => tr.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
