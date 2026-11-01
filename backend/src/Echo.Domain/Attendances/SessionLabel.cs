using Echo.Domain.Congregations;

namespace Echo.Domain.Attendances;

public class SessionLabel : ISoftDeletable
{
    public int Id { get; set; }

    public Guid CongregationId { get; set; }

    /// <summary>User-facing name. Congregations can rename freely.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Internal category. Only assigned on object creation. <b>Not editable</b></summary>
    public SessionCategory Category { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
