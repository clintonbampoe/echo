using Api.Core.Data.EntityConfigurations.Base;
using Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Core.Data.EntityConfigurations;

public class TransactionConfiguration : PrimaryEntityConfigurationBase<Transaction>
{
    public override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.CategoryId);
    }
}
