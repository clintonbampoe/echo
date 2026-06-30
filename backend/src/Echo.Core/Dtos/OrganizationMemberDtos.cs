using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record OrganizationMemberCreateDto : IPrimaryCreateDto
{
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberUpdateDto : IPrimaryUpdateDto
{
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string OrganizationName { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
