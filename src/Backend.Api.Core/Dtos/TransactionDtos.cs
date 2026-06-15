using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TransactionCreateDto(
    int CategoryId,
    TransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount,
    string? Description
) : IPrimaryCreateDto<Transaction>;

public record TransactionUpdateDto(
    int CategoryId,
    TransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount,
    string? Description
) : IPrimaryUpdateDto<Transaction>;

public record TransactionListResponseDto(
    Guid Id,
    string CategoryName,
    TransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount
) : IPrimaryListResponseDto<Transaction>;

public record TransactionResponseDto(
    Guid Id,
    int CategoryId,
    string CategoryName,
    TransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto<Transaction>;
