namespace Backend.Api.Core.Entities;

public class ProjectCategory : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Title { get; init; } = string.Empty;
}
