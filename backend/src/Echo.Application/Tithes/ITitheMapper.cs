using Echo.Domain.Tithes;

namespace Echo.Application.Tithes;

public interface ITitheMapper
{
    TitheResponseDto ToDto(Tithe entity);
    Tithe ToEntity(TitheCreateDto dto);
    List<TitheResponseDto> ToListDto(List<Tithe> entities);

    void Patch(TitheUpdateDto dto, Tithe entity);
}
