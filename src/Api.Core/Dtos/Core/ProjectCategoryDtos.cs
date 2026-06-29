using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record ProjectCategoryCreateDto(string Name) : IReferenceCreateDto;

public record ProjectCategoryUpdateDto(string Name) : IReferenceUpdateDto;

public record ProjectCategoryResponseDto(int Id, string Name) : IReferenceResponseDto;
