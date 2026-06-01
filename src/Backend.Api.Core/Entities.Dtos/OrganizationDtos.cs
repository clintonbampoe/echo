using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

public record OrganizationCreateDto
    (Guid Id,
    Guid CongregationId,
    string Name,
    DateTime CreatedAt,
    string? Description
        ) : ICreateDto<Organization>;

public record OrganizationResponseDto
    (Guid Id,
    Guid CongregationId,
    string Name,
    string? Description
    ) : IResponseDto<Organization>;

public record OrganizationListResponseDto
    (Guid Id,
    Guid CongregationId,
    string Name
    ) : IResponseDto<Organization>;

public record OrganizationUpdateDto
    (Guid Id,
    Guid CongregationId,
    string Name,
    string? Description
        ) : IUpdateDto<Organization>;

public record OrganizationDeleteDto
    (Guid Id,
    Guid CongregationId
        ) : ISoftDeleteDto<Organization>;
