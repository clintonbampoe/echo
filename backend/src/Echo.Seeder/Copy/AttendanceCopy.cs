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
                "AttendanceTypeId", "PersonId",
                "Date", "CheckInTime",
                "Notes", "CreatedAt"
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var a in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(a.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.AttendanceTypeId, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(a.PersonId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.Date, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(a.CheckInTime, NpgsqlDbType.Time, ct);
            await WriteNullableText(writer, a.Notes, ct);
            await writer.WriteAsync(a.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
