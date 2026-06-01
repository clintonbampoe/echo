using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

public record MemberListResponseDto : IResponseDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string FullName { get; init; } = null!;
}
