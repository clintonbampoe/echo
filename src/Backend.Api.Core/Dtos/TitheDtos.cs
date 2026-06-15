using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TitheCreateDto(
    Guid MemberId,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate,
    string? Description
) : IPrimaryCreateDto<Tithe>;

public record TitheUpdateDto(
    Guid MemberId,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate,
    string? Description
) : IPrimaryUpdateDto<Tithe>;

public record TitheListResponseDto(
    Guid Id,
    string MemberName,
    decimal Amount,
    int ForYear,
    MonthOfYear ForMonth,
    PaymentMethod PaymentMethod,
    DateOnly CollectionDate
) : IPrimaryListResponseDto<Tithe>;

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
) : IPrimaryResponseDto<Tithe>;
