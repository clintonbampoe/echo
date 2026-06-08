using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record OrganizationMemberCreateDto : ICreateDto<OrganizationMember>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberResponseDto : IResponseDto<OrganizationMember>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public string Member { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
    public string Organization { get; init; } = string.Empty;
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberListResponseDto : IListResponseDto<OrganizationMember>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Member { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberUpdateDto : IUpdateDto<OrganizationMember>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberDeleteDto : ISoftDeleteDto<OrganizationMember>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
