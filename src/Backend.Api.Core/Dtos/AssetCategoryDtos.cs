using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record AssetCategoryCreateDto(string Name) : IReferenceCreateDto<AssetCategory>;

public record AssetCategoryUpdateDto(string Name) : IReferenceUpdateDto<AssetCategory>;

public record AssetCategoryResponseDto(int Id, string Name) : IReferenceResponseDto<AssetCategory>;
