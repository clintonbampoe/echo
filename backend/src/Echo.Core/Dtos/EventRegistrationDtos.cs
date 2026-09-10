namespace Echo.Core.Dtos;

public record EventRegistrationCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationUpdateDto
{
    public DateOnly? RegistrationDate { get; init; }
}

public record EventRegistrationListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string EventName { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public DateOnly RegistrationDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
