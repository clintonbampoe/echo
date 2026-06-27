using Api.Core.Dtos.Interfaces;

namespace Api.Core.Dtos;

public record AssetCategoryCreateDto(string Name) : IReferenceCreateDto;

public record AssetCategoryUpdateDto(string Name) : IReferenceUpdateDto;

public record AssetCategoryResponseDto(int Id, string Name) : IReferenceResponseDto;
