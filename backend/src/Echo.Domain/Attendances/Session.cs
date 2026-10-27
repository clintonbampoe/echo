using Echo.Domain.Users;

namespace Echo.Domain.Attendances;

public class Session : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public int SessionLabelId { get; set; }
    public SessionLabel SessionLabel { get; set; } = null!;

    public DateOnly Date { get; set; }
    public string? Name { get; set; }
    public string? Topic { get; set; }
    public string? Notes { get; set; }

    /// <summary>Where it is held, for example "Main hall".</summary>
    public string? Location { get; set; }
    public TimeOnly? StartsAt { get; set; }
    public TimeOnly? EndsAt { get; set; }
    public SessionStatus Status { get; set; }
    public Guid? CreatedBy { get; set; }
    public User? Creator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
