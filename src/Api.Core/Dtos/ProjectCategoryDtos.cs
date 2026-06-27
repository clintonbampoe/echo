using Api.Core.Dtos.Interfaces;

namespace Api.Core.Dtos;

public record ProjectCategoryCreateDto(string Name) : IReferenceCreateDto;

public record ProjectCategoryUpdateDto(string Name) : IReferenceUpdateDto;

public record ProjectCategoryResponseDto(int Id, string Name) : IReferenceResponseDto;
