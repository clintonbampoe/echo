using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record OrganizationMemberCreateDto(
    Guid MemberId,
    Guid OrganizationId,
    MemberOrganizationalRole Role,
    DateOnly JoinedAt
) : IPrimaryCreateDto;

public record OrganizationMemberUpdateDto(MemberOrganizationalRole Role, DateOnly JoinedAt)
    : IPrimaryUpdateDto;

public record OrganizationMemberListResponseDto(
    Guid Id,
    string MemberName,
    string OrganizationName,
    MemberOrganizationalRole Role,
    DateOnly JoinedAt
) : IPrimaryListResponseDto;

public record OrganizationMemberResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid OrganizationId,
    string OrganizationName,
    MemberOrganizationalRole Role,
    DateOnly JoinedAt,
    DateTime CreatedAt
) : IPrimaryResponseDto;
