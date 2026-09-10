using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record OrganizationMemberCreateDto
{
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberUpdateDto
{
    public MemberOrganizationalRole? Role { get; init; }
    public DateOnly? JoinedAt { get; init; }
}

public record OrganizationMemberListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string OrganizationName { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberResponseDto
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
