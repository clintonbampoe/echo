using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsForTheFrontendBoards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_attendance_session_id_person_id",
                table: "attendance");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "visitors",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "invited_by_member_id",
                table: "visitors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "visitors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "sessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "baptised_date",
                table: "members",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_visitors_invited_by_member_id",
                table: "visitors",
                column: "invited_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions",
                columns: new[] { "congregation_id", "transaction_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "transaction_type", "category_id", "amount", "payment_method" });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_session_id_person_id",
                table: "attendance",
                columns: new[] { "session_id", "person_id" },
                unique: true,
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "person_kind_at_time", "is_first_visit" });

            migrationBuilder.AddForeignKey(
                name: "fk_visitors_members_invited_by_member_id",
                table: "visitors",
                column: "invited_by_member_id",
                principalTable: "members",
                principalColumn: "person_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_visitors_members_invited_by_member_id",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_visitors_invited_by_member_id",
                table: "visitors");

            migrationBuilder.DropIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_attendance_session_id_person_id",
                table: "attendance");

            migrationBuilder.DropColumn(
                name: "address",
                table: "visitors");

            migrationBuilder.DropColumn(
                name: "invited_by_member_id",
                table: "visitors");

            migrationBuilder.DropColumn(
                name: "source",
                table: "visitors");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "location",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "baptised_date",
                table: "members");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_congregation_id_transaction_date_id",
                table: "transactions",
                columns: new[] { "congregation_id", "transaction_date", "id" },
                descending: new[] { false, true, false },
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "transaction_type", "category_id", "amount" });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_session_id_person_id",
                table: "attendance",
                columns: new[] { "session_id", "person_id" },
                unique: true,
                filter: "deleted_at IS NULL");
        }
    }
}
