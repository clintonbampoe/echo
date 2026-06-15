using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record EventRegistrationCreateDto(Guid MemberId, Guid EventId, DateOnly RegistrationDate)
    : IPrimaryCreateDto<EventRegistration>;

public record EventRegistrationUpdateDto(DateOnly RegistrationDate)
    : IPrimaryUpdateDto<EventRegistration>;

public record EventRegistrationListResponseDto(
    Guid Id,
    string MemberName,
    string EventName,
    DateOnly RegistrationDate
) : IPrimaryListResponseDto<EventRegistration>;

public record EventRegistrationResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid EventId,
    string EventName,
    DateOnly RegistrationDate,
    DateTime CreatedAt
) : IPrimaryResponseDto<EventRegistration>;
