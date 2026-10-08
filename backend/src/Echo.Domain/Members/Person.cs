using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Person : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public PersonKind Kind { get; set; }

    public string Name { get; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OtherNames { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? EmailAddress { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
