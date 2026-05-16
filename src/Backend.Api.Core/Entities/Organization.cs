namespace Backend.Api.Core.Entities;

public class Organization
{
    public int OrganizationId { get; init; }
    public Guid UniqueId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateOnly CreatedAt { get; init; }
}
