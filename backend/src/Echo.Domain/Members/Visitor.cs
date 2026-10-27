using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Visitor : ISoftDeletable
{
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public Guid CongregationId { get; set; }

    public string? Notes { get; set; }

    /// <summary>Visitors give their gender, not their age. Empty for visitors recorded before it was asked.</summary>
    public Gender? Gender { get; set; }

    public string? Address { get; set; }

    /// <summary>How they came. Empty for visitors recorded before it was asked.</summary>
    public VisitorSource? Source { get; set; }

    /// <summary>The member who invited them, when they came by invitation and it is known.</summary>
    public Guid? InvitedByMemberId { get; set; }
    public Member? InvitedBy { get; set; }

    public DateTime? ConvertedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
