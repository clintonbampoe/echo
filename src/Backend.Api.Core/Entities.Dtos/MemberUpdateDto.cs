using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

public record MemberUpdateDto : IUpdateDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string? FirstName { get; init; } = null!;
    public string? LastName { get; init; } = null!;
    public DateTime? DateOfBirth { get; init; }
}
