using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Echo.Data;

public static class DbExceptions
{
    /// <summary>
    /// True when a save failed because it would break a unique index, for example two requests
    /// marking the same person present at the same moment.
    /// </summary>
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
