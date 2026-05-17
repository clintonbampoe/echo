namespace Backend.Api.Core.Entities;

public class ProjectCategory
{
    public int CategoryId { get; init; }
    public Guid UniqueId { get; init; }
    public string Title { get; init; } = string.Empty;
}
