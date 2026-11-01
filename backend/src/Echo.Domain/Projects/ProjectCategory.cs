using Echo.Domain.Congregations;

namespace Echo.Domain.Projects;

public class ProjectCategory : ISearchable, ISoftDeletable
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
