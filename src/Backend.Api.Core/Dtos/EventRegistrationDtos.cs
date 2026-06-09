using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record EventRegistrationCreateDto : ICreateDto<EventRegistration>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationResponseDto : IResponseDto<EventRegistration>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public string Member { get; init; } = string.Empty;
    public Guid EventId { get; init; }
    public string Event { get; init; } = string.Empty;
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationListResponseDto : IListResponseDto<EventRegistration>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Member { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationUpdateDto : IUpdateDto<EventRegistration>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationDeleteDto : ISoftDeleteDto<EventRegistration>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
