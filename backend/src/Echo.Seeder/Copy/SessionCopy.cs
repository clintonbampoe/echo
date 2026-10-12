using Echo.Domain.Attendances;
using Npgsql;
using NpgsqlTypes;
using static Echo.Seeder.Copy.PeopleCopy;

namespace Echo.Seeder.Copy;

public static class SessionsCopy
{
    public static async Task WriteSessionsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Session> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY "Sessions" (
                "Id", "CongregationId", "SessionLabelId",
                "Date", "Name", "Topic", "Notes",
                "StartsAt", "EndsAt",
                "Status", "CreatedBy", "CreatedAt"
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var s in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(s.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(s.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(s.SessionLabelId, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(s.Date, NpgsqlDbType.Date, ct);
            await WriteNullableText(writer, s.Name, ct);
            await WriteNullableText(writer, s.Topic, ct);
            await WriteNullableText(writer, s.Notes, ct);
            await WriteNullableTime(writer, s.StartsAt, ct);
            await WriteNullableTime(writer, s.EndsAt, ct);
            await writer.WriteAsync(s.Status.ToString(), NpgsqlDbType.Text, ct);
            await WriteNullableUuid(writer, s.CreatedBy, ct);
            await writer.WriteAsync(s.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
