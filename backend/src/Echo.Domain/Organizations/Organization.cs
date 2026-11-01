using Echo.Domain.Congregations;

namespace Echo.Domain.Organizations;

public class Organization : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public OrganizationType Kind { get; set; } = OrganizationType.Other;

    /// <summary>The day of the week the organization usually meets, if it has a regular day.</summary>
    public DayOfWeek? MeetingDay { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
