using Api.Core.Dtos.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos.Core;

public record TransactionCategoryCreateDto(string Name, TransactionType CategoryType)
    : IReferenceCreateDto;

public record TransactionCategoryUpdateDto(string Name, TransactionType CategoryType)
    : IReferenceUpdateDto;

public record TransactionCategoryResponseDto(int Id, string Name, TransactionType CategoryType)
    : IReferenceResponseDto;
