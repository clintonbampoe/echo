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

public record MemberListResponseDto
    (Guid Id,
    Guid CongregationId,
    string Name
    ) : IListResponseDto<Member>;

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
