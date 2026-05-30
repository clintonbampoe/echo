using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.HasOne<TransactionCategory>()
            .WithMany()
            .HasForeignKey(tr => tr.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
