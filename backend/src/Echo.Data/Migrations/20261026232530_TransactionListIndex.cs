using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class TransactionListIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_congregation_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_transactions_transaction_date_id",
                table: "transactions");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions",
                columns: new[] { "congregation_id", "transaction_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "transaction_type", "category_id", "amount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_congregation_id",
                table: "transactions",
                column: "congregation_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_transaction_date_id",
                table: "transactions",
                columns: new[] { "transaction_date", "id" },
                filter: "deleted_at IS NULL");
        }
    }
}
