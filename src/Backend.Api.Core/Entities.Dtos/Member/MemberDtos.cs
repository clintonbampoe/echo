using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

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
    string FirstName,
    string LastName,
    string OtherNames,
    string Name,
    DateOnly? DateOfBirth,
    string? Email,
    string? PhoneNumber
    ) : IResponseDto<Member>;

public record MemberListResponseDto
    (Guid Id,
    Guid CongregationId,
    string FullName
    ) : IResponseDto<Member>;

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
