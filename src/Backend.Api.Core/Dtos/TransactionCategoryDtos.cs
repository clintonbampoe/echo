using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TransactionCategoryCreateDto(string Name, TransactionType CategoryType)
    : IReferenceCreateDto<TransactionCategory>;

public record TransactionCategoryUpdateDto(string Name, TransactionType CategoryType)
    : IReferenceUpdateDto<TransactionCategory>;

public record TransactionCategoryResponseDto(int Id, string Name, TransactionType CategoryType)
    : IReferenceResponseDto<TransactionCategory>;
