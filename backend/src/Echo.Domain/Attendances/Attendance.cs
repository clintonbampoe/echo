using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Users;

namespace Echo.Domain.Attendances;

public class Attendance : ISoftDeletable
{
    public Guid Id { get; set; }

    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    /// <summary>
    /// The person's Kind at the moment this attendance was recorded.
    /// <br/><br/>
    /// Snapshotted because Person.Kind reflects the person's *current* role,
    /// which changes over time. A visitor who later becomes a member must
    /// still show as a visitor for the attendances they made before
    /// converting. Without this, historical reports mislabel the past.
    /// <br/><br/>
    /// Set at record creation from Person.Kind. Never updated afterwards.
    /// </summary>
    public PersonKind PersonKindAtTime { get; set; }

    /// <summary>
    /// Exact moment the check-in occurred. Null for manual entries where only
    /// presence matters. Populated by QR-scan self check-in.
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// Admin who recorded the attendance. Null for self check-in.
    /// </summary>
    public Guid? RecordedBy { get; set; }
    public User? Recorder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
