using Echo.Domain.Attendances;
using Npgsql;
using NpgsqlTypes;
using static Echo.Seeder.Copy.PeopleCopy;

namespace Echo.Seeder.Copy;

public static class AttendanceCopy
{
    public static async Task WriteAttendancesAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Attendance> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY "Attendance" (
                "Id", "CongregationId",
                "SessionId", "PersonId",
                "PersonKindAtTime",
                "CheckedInAt", "RecordedBy",
                "CreatedAt"
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var a in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(a.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.SessionId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.PersonId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.PersonKindAtTime.ToString(), NpgsqlDbType.Text, ct);
            await WriteNullableTimestamp(writer, a.CheckedInAt, ct);
            await WriteNullableUuid(writer, a.RecordedBy, ct);
            await writer.WriteAsync(a.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
