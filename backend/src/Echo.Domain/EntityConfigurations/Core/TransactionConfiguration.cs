using Echo.Domain.Entities.Core;
using Echo.Domain.EntityConfigurations.Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Domain.EntityConfigurations.Core;

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

        builder
            .HasIndex(t => new { t.TransactionDate, t.Id })
            .HasFilter($"\"{nameof(Transaction.DeletedAt)}\" IS NULL");
    }
}
