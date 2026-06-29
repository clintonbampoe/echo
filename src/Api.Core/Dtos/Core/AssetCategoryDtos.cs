using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record AssetCategoryCreateDto(string Name) : IReferenceCreateDto;

public record AssetCategoryUpdateDto(string Name) : IReferenceUpdateDto;

public record AssetCategoryResponseDto(int Id, string Name) : IReferenceResponseDto;
