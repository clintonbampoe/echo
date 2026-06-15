using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record ProjectCategoryCreateDto(string Name) : IReferenceCreateDto<ProjectCategory>;

public record ProjectCategoryUpdateDto(string Name) : IReferenceUpdateDto<ProjectCategory>;

public record ProjectCategoryResponseDto(int Id, string Name)
    : IReferenceResponseDto<ProjectCategory>;
