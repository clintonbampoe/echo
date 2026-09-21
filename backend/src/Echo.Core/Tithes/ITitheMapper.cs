using Echo.Domain.Tithes;

namespace Echo.Core.Tithes;

public interface ITitheMapper
{
    TitheResponseDto ToDto(Tithe entity);
    Tithe ToEntity(TitheCreateDto dto);
    List<TitheResponseDto> ToListDto(List<Tithe> entities);

    void Patch(TitheUpdateDto dto, Tithe entity);
}
