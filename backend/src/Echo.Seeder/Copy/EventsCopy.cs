using Echo.Domain.Events;
using Npgsql;
using NpgsqlTypes;
using static Echo.Seeder.Copy.PeopleCopy;

namespace Echo.Seeder.Copy;

public static class EventsCopy
{
    public static async Task WriteEventsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Event> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY events (
                id, congregation_id,
                organization_id, organizer_id,
                name, start_date, end_date,
                start_time, end_time,
                location, capacity, description,
                created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var e in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(e.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(e.OrganizationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(e.OrganizerId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(e.Name, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(e.StartDate, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(e.EndDate, NpgsqlDbType.Date, ct);
            await WriteNullableTime(writer, e.StartTime, ct);
            await WriteNullableTime(writer, e.EndTime, ct);
            await WriteNullableText(writer, e.Location, ct);
            await WriteNullableInt(writer, e.Capacity, ct);
            await WriteNullableText(writer, e.Description, ct);
            await writer.WriteAsync(e.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteEventRegistrationsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<EventRegistration> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY event_registrations (
                id, congregation_id,
                member_id, event_id,
                registration_date, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var r in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(r.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(r.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(r.MemberId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(r.EventId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(r.RegistrationDate, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(r.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteEventAttendancesAsync(
        NpgsqlConnection conn,
        IReadOnlyList<EventAttendance> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY event_attendances (
                id, congregation_id,
                member_id, event_id,
                check_in_time, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var a in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(a.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.MemberId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.EventId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(a.CheckInTime, NpgsqlDbType.Time, ct);
            await writer.WriteAsync(a.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
