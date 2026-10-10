using Echo.Domain.Organizations;
using Npgsql;
using NpgsqlTypes;
using static Echo.Seeder.Copy.PeopleCopy;

namespace Echo.Seeder.Copy;

public static class OrganizationsCopy
{
    public static async Task WriteOrganizationsAsync(
        NpgsqlConnection conn,
        IReadOnlyList<Organization> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY "Organizations" (
                "Id", "CongregationId", "Name", "Description", "CreatedAt"
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var o in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(o.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(o.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(o.Name, NpgsqlDbType.Text, ct);
            await WriteNullableText(writer, o.Description, ct);
            await writer.WriteAsync(o.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }

    public static async Task WriteOrganizationMembersAsync(
        NpgsqlConnection conn,
        IReadOnlyList<OrganizationMember> batch,
        CancellationToken ct
    )
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY "OrganizationMembers" (
                "Id", "CongregationId",
                "MemberId", "OrganizationId",
                "Role", "JoinedAt", "CreatedAt"
            ) FROM STDIN (FORMAT BINARY)
            """,
            ct
        );

        foreach (var om in batch)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(om.Id, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(om.CongregationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(om.MemberId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(om.OrganizationId, NpgsqlDbType.Uuid, ct);
            await writer.WriteAsync(om.Role.ToString(), NpgsqlDbType.Text, ct);
            await writer.WriteAsync(om.JoinedAt, NpgsqlDbType.Date, ct);
            await writer.WriteAsync(om.CreatedAt, NpgsqlDbType.TimestampTz, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
