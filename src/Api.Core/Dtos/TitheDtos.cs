using Api.Core.Dtos.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos;

public record TitheCreateDto(
    Guid MemberId,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate,
    string? Description
) : IPrimaryCreateDto;

public record TitheUpdateDto(
    Guid MemberId,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate,
    string? Description
) : IPrimaryUpdateDto;

public record TitheListResponseDto(
    Guid Id,
    string MemberName,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate
) : IPrimaryListResponseDto;

public record TitheResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
