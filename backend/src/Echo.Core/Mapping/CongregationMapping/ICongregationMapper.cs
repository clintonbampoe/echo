using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.CongregationMapping;

public interface ICongregationMapper
{
    CongregationResponseDto ToDto(Congregation entity);
    Congregation ToEntity(CongregationCreateDto dto);
    List<CongregationResponseDto> ToListDto(List<Congregation> entities);

    List<CongregationSearchResultDto> ToSearchDto(List<Congregation> entities);
    void Patch(CongregationUpdateDto dto, Congregation entity);
}
