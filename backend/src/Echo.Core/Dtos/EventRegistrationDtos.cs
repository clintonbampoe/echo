using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record EventRegistrationCreateDto : IPrimaryCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationUpdateDto : IPrimaryUpdateDto
{
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string EventName { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public DateOnly RegistrationDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
