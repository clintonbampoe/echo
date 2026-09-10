using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.EventMapping;

public interface IEventMapper
{
    EventResponseDto ToDto(Event entity);
    Event ToEntity(EventCreateDto dto);
    List<EventResponseDto> ToListDto(List<Event> entities);

    List<EventSearchResultDto> ToSearchDto(List<Event> entities);
    void Patch(EventUpdateDto dto, Event entity);
}
