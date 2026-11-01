using Echo.Domain.Members;
using Npgsql;
using NpgsqlTypes;

namespace Echo.Seeder.Copy;

/// <summary>
/// Binary COPY writers for the people tables.
///
/// Column order must match the COPY statement exactly — this is the contract
/// between the seeder and the database. If you add or rename a column in a
/// migration, update the COPY statement and the Write calls here to match.
///
/// Enums are stored as their string name (configured via
/// configurationBuilder.Properties&lt;Enum&gt;().HaveConversion&lt;string&gt;() in AppDbContext).
///
/// Name is a stored computed column — never written directly.
/// DeletedAt is never set by the seeder (all seeded data is active).
/// CreatedAt has a DB default (now()) but we write it explicitly so the value
/// matches ReferenceNow rather than the time the seeder runs.
/// </summary>
public static class PeopleCopy
{
    public static async Task WritePersonsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Person> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY people (
                id, congregation_id, kind,
                first_name, last_name, other_names,
                phone_number, email_address,
                created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var p in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(p.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(p.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(p.Kind.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(p.FirstName, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(p.LastName, NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, p.OtherNames, ct);
            await writer.WriteAsync(p.PhoneNumber, NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, p.EmailAddress, ct);
            await writer.WriteAsync(p.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteMembersAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Member> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY members (
                person_id, congregation_id,
                date_of_birth, joined_date, gender,
                residential_address, city, hometown, region, gps_address,
                marital_status, next_of_kin,
                emergency_contact_name, emergency_contact_phone_number,
                status, created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var m in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(m.PersonId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(m.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(m.DateOfBirth, NpgsqlDbType.Date, ct);
            await WriteNullableDate(writer, m.JoinedDate, ct);
            await writer.WriteAsync(m.Gender.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.ResidentialAddress, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.City, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.Hometown, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.Region.ToString(), NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, m.GpsAddress, ct);
            await writer.WriteAsync(m.MaritalStatus.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.NextOfKin, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.EmergencyContactName, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.EmergencyContactPhoneNumber, NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.Status.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(m.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteVisitorsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Visitor> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY visitors (
                person_id, congregation_id,
                gender, source, notes, converted_at,
                created_at
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var v in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(v.PersonId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(v.CongregationId, NpgsqlDbType.Uuid, ct);
            await WriteNullableText(writer, v.Gender?.ToString(), ct);
            await WriteNullableText(writer, v.Source?.ToString(), ct);
            await WriteNullableText(writer, v.Notes, ct);
            await WriteNullableTimestamp(writer, v.ConvertedAt, ct);
            await writer.WriteAsync(v.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    // ── Nullable helpers ──────────────────────────────────────────────────
    // Npgsql's binary importer has no built-in nullable helpers, so we add
    // thin wrappers. Each one writes NULL or the value.

    internal static async Task WriteNullableText(
        NpgsqlBinaryImporter w,
        string? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value, NpgsqlDbType.Text, ct);
    }

    internal static async Task WriteNullableDate(
        NpgsqlBinaryImporter w,
        DateOnly? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value.Value, NpgsqlDbType.Date, ct);
    }

    internal static async Task WriteNullableUuid(
        NpgsqlBinaryImporter w,
        Guid? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value.Value, NpgsqlDbType.Uuid, ct);
    }

    internal static async Task WriteNullableTimestamp(
        NpgsqlBinaryImporter w,
        DateTime? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value.Value, NpgsqlDbType.TimestampTz, ct);
    }

    internal static async Task WriteNullableInt(
        NpgsqlBinaryImporter w,
        int? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value.Value, NpgsqlDbType.Integer, ct);
    }

    internal static async Task WriteNullableTime(
        NpgsqlBinaryImporter w,
        TimeOnly? value,
        CancellationToken ct
    )
    {
        if (value is null)
            await w.WriteNullAsync(ct);
        else
            await w.WriteAsync(value.Value, NpgsqlDbType.Time, ct);
    }
}
