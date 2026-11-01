using Echo.Domain.Congregations;
using Echo.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echo.Data.Configurations.Transactions;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder
            .HasOne<Congregation>()
            .WithMany()
            .HasForeignKey(e => e.CongregationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.CategoryId);

        // Serves the list (newest first, cursor on transaction_date + id) and the summaries as
        // index-only scans.
        builder
            .HasIndex(t => new
            {
                t.CongregationId,
                t.TransactionDate,
                t.Id,
            })
            .IsDescending(false, true, false)
            .IncludeProperties(t => new
            {
                t.TransactionType,
                t.CategoryId,
                t.Amount,
                t.PaymentMethod,
            })
            .HasFilter("deleted_at IS NULL");
    }
}
