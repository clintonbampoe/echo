using Echo.Domain.Congregations;

namespace Echo.Core.Congregations;

public interface ICongregationMapper
{
    CongregationResponseDto ToDto(Congregation entity);
    Congregation ToEntity(CongregationCreateDto dto);
    List<CongregationResponseDto> ToListDto(List<Congregation> entities);

    List<CongregationSearchResultDto> ToSearchDto(List<Congregation> entities);
    void Patch(CongregationUpdateDto dto, Congregation entity);
}
