using Backend.Api.Core.Data.EntityConfigurations.Interfaces;
using Backend.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Core.Data.EntityConfigurations;

public class TransactionConfiguration : CongregationEntityConfigurationBase<Transaction>
{
    public override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.HasOne(tr => tr.Category)
            .WithMany()
            .HasForeignKey(tr => tr.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
