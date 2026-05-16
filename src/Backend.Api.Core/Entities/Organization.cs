namespace Backend.Api.Core.Entities;

public class Organization
{
    public int OrganizationId { get; }
    public Guid UniqueId { get; }
    public string Name { get; }
    public string Description { get; }
    public DateOnly CreatedAt { get; }
}
