using Echo.Domain.Organizations;

namespace Echo.Core.Organizations;

public record OrganizationMemberCreateDto
{
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
}

public record OrganizationMemberUpdateDto
{
    public MemberRole? Role { get; init; }
    public DateOnly? JoinedAt { get; init; }
}

public record OrganizationMemberResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public MemberRole Role { get; init; }
    public DateOnly JoinedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrganizationMemberCursor
{
    public DateTime CreatedAt { get; init; }
    public Guid Id { get; init; }
}

public record OrganizationMemberFilters
{
    public MemberRole? Role { get; init; }
}
