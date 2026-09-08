using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.CongregationMapping;

public interface ICongregationMapper
{
    CongregationResponseDto ToDto(Congregation entity);
    Congregation ToEntity(CongregationCreateDto dto);
    void Patch(CongregationUpdateDto dto, Congregation entity);
}
