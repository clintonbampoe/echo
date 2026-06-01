namespace Backend.Api.Core.Entities.Dtos;

public record MemberSoftDeleteDto : ISoftDeleteDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
