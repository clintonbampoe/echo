using Echo.Domain.Congregations;
using Echo.Domain.Users;

namespace Echo.Domain.Attendances;

public class Session : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;
    public int SessionLabelId { get; set; }
    public SessionLabel SessionLabel { get; set; } = null!;
    public DateOnly Date { get; set; }
    public string? Name { get; set; }
    public string? Topic { get; set; }
    public string? Notes { get; set; }
    public TimeOnly? StartsAt { get; set; }
    public TimeOnly? EndsAt { get; set; }
    public SessionStatus Status { get; set; }
    public Guid? CreatedBy { get; set; }
    public User? Creator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
