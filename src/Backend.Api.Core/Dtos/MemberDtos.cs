using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record MemberCreateDto
    (Guid Id,
    Guid CongregationId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
        ) : ICreateDto<Member>;

public record MemberResponseDto
    (Guid Id,
    Guid CongregationId,
    string Name,
    DateOnly? DateOfBirth,
    string? Email,
    string? PhoneNumber
    ) : IResponseDto<Member>;

public record MemberListResponseDto : IListResponseDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; set; }
    public string Name { get; set; } = string.Empty;
}
public record MemberUpdateDto
    (Guid Id,
    Guid CongregationId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
        ) : IUpdateDto<Member>;


public record MemberDeleteDto
    (Guid Id,
    Guid CongregationId
        ) : ISoftDeleteDto<Member>;
