using Echo.Domain.Assets;
using Echo.Domain.Projects;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Npgsql;
using NpgsqlTypes;
using static Echo.Seeder.Copy.PeopleCopy;

namespace Echo.Seeder.Copy;

public static class FinancesCopy
{
    public static async Task WriteTithesAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Tithe> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY tithes (
                id, congregation_id, member_id,
                amount, for_year, for_month,
                payment_method, collection_date,
                description, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var t in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(t.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(t.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(t.MemberId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(t.Amount, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(t.ForYear, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(t.ForMonth.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(t.PaymentMethod.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(t.CollectionDate, NpgsqlDbType.Date, ct);
            await WriteNullableText(writer, t.Description, ct);
            await writer.WriteAsync(t.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteTransactionsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Transaction> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY transactions (
                id, congregation_id, category_id,
                transaction_type, transaction_date,
                amount, description, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var t in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(t.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(t.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(t.CategoryId, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(t.TransactionType.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(t.TransactionDate, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(t.Amount, NpgsqlDbType.Numeric, ct);
            await WriteNullableText(writer, t.Description, ct);
            await writer.WriteAsync(t.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteProjectsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Project> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY projects (
                id, congregation_id, category_id, manager_id,
                name, target_amount, status,
                start_date, end_date,
                description, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var p in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(p.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(p.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(p.CategoryId, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(p.ManagerId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(p.Name, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(p.TargetAmount, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(p.Status.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(p.StartDate, NpgsqlDbType.Date, ct);
            await WriteNullableDate(writer, p.EndDate, ct);
            await WriteNullableText(writer, p.Description, ct);
            await writer.WriteAsync(p.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteProjectContributionsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<ProjectContribution> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY project_contributions (
                id, congregation_id, project_id,
                amount, date_contributed,
                payment_method, description, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var c in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(c.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(c.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(c.ProjectId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(c.Amount, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(c.DateContributed, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(c.PaymentMethod.ToString(), NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, c.Description, ct);
            await writer.WriteAsync(c.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteAssetsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Asset> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY assets (
                id, congregation_id, category_id,
                name, serial_number,
                purchase_date, purchase_cost, current_value,
                status, description, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var a in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(a.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CategoryId, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(a.Name, NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, a.SerialNumber, ct);
            await WriteNullableDate(writer, a.PurchaseDate, ct);
            await writer.WriteAsync(a.PurchaseCost, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(a.CurrentValue, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(a.Status.ToString(), NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, a.Description, ct);
            await writer.WriteAsync(a.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
