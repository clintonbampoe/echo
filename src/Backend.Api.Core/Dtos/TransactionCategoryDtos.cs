using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TransactionCategoryCreateDto(string Name, TransactionType CategoryType)
    : IReferenceCreateDto;

public record TransactionCategoryUpdateDto(string Name, TransactionType CategoryType)
    : IReferenceUpdateDto;

public record TransactionCategoryResponseDto(int Id, string Name, TransactionType CategoryType)
    : IReferenceResponseDto;
