using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.TitheMapping;

public interface ITitheMapper
{
    TitheResponseDto ToDto(Tithe entity);
    Tithe ToEntity(TitheCreateDto dto);
    List<TitheResponseDto> ToListDto(List<Tithe> entities);

    void Patch(TitheUpdateDto dto, Tithe entity);
}
