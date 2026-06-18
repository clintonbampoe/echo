using Backend.Api.Core.Dtos.Interfaces;

namespace Backend.Api.Core.Dtos;

public record EventRegistrationCreateDto(Guid MemberId, Guid EventId, DateOnly RegistrationDate)
    : IPrimaryCreateDto;

public record EventRegistrationUpdateDto(DateOnly RegistrationDate) : IPrimaryUpdateDto;

public record EventRegistrationListResponseDto(
    Guid Id,
    string MemberName,
    string EventName,
    DateOnly RegistrationDate
) : IPrimaryListResponseDto;

public record EventRegistrationResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid EventId,
    string EventName,
    DateOnly RegistrationDate,
    DateTime CreatedAt
) : IPrimaryResponseDto;
