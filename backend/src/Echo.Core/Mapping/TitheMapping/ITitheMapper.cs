using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.TitheMapping;

public interface ITitheMapper
{
    TitheResponseDto ToDto(Tithe entity);
    Tithe ToEntity(TitheCreateDto dto);
    void Patch(TitheUpdateDto dto, Tithe entity);
}
