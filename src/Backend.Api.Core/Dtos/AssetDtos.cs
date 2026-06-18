using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record AssetCreateDto(
    int CategoryId,
    string Name,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    decimal PurchaseCost,
    decimal CurrentValue,
    AssetStatus Status,
    string? Description
) : IPrimaryCreateDto;

public record AssetUpdateDto(
    int CategoryId,
    string Name,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    decimal PurchaseCost,
    decimal CurrentValue,
    AssetStatus Status,
    string? Description
) : IPrimaryUpdateDto;

public record AssetListResponseDto(
    Guid Id,
    string CategoryName,
    string Name,
    AssetStatus Status,
    decimal CurrentValue
) : IPrimaryListResponseDto;

public record AssetResponseDto(
    Guid Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    decimal PurchaseCost,
    decimal CurrentValue,
    AssetStatus Status,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
