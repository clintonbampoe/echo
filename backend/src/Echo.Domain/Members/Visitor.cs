using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Visitor : ISoftDeletable
{
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string? Notes { get; set; }

    public Guid? ConvertedToMemberPersonId { get; set; }
    public Member? ConvertedToMember { get; set; }
    public DateTime? ConvertedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
