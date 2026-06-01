using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

public record MemberResponseDto : IResponseDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? OtherNames { get; init; }
    public string Name => $"{FirstName} {LastName}";
    public DateOnly? DateOfBirth { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
