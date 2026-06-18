using Backend.Api.Core.Dtos.Interfaces;

namespace Backend.Api.Core.Dtos;

public record AssetCategoryCreateDto(string Name) : IReferenceCreateDto;

public record AssetCategoryUpdateDto(string Name) : IReferenceUpdateDto;

public record AssetCategoryResponseDto(int Id, string Name) : IReferenceResponseDto;
