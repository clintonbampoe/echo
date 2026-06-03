using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;


public record OrganizationMemberResponseDto
    (
        Guid Id,
        Guid CongregationId,
        string MemberName,
        string OrganizationName,
        MemberOrganizationalRole Role,
        DateOnly JoinedAt
    ) : IResponseDto<OrganizationMember>;

public record OrganizationMemberListResponseDto
    (
        Guid Id,
        Guid CongregationId,
        string MemberName,
        string OrganizationName
    ) : IListResponseDto<OrganizationMember>;

public record OrganizationMemberCreateDto
    (
        Guid Id,
        Guid CongregationId,
        Guid MemberId,
        Guid OrganizationId,
        MemberOrganizationalRole Role,
        DateOnly JoinedAt
    ) : ICreateDto<OrganizationMember>;

public record OrganizationMemberUpdateDto
    (
        Guid Id,
        Guid CongregationId,
        MemberOrganizationalRole Role,
        DateOnly JoinedAt
    ) : IUpdateDto<OrganizationMember>;

public record OrganizationMemberDeleteDto
    (
        Guid Id,
        Guid CongregationId
    ) : ISoftDeleteDto<OrganizationMember>;
